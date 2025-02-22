using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using curry.Common;
using UnityEngine.Audio;

namespace curry.Sound
{
    public class SEPlayer : SingletonMonoBehaviour<SEPlayer>
    {
        private class RequestData
        { 
            public string       m_AssetName;
            public bool         m_IsLoop;
            public float        m_Duration;
            public AudioSource  m_TargetAudioSouce;
        }

        public const int kSplitMax = 20;
        public const float kMaxVolume = 0.15f;
        private static float CurrentVolume { get; set; } = kMaxVolume;
        private static List<AudioSource> AudioSources { get; } = new ();
        private static AudioMixer AudioMixer { get; set; }

        private const int kAudioSourceCountMax = 10;

        // 同じSEを同じフレームで再生させる場合の最大数
        private const int kSameSECountMax = 1;

        private static List<RequestData> m_Queue = new();

        private new void Awake()
        {
            base.Awake();
            DontDestroyOnLoad();
        }

        private void LateUpdate()
        {
            if (!m_Queue.Any())
            {
                return;
            }

            foreach (var reqInfo in m_Queue)
            {
                PlayStart(reqInfo.m_AssetName, reqInfo.m_IsLoop, CurrentVolume, reqInfo.m_Duration, reqInfo.m_TargetAudioSouce).Forget();
            }

            m_Queue.Clear();
        }

        public static void VolumeSet(int volume)
        {
            CurrentVolume = kMaxVolume * (volume / (float)kSplitMax);
            AudioMixer = HoldAssetLoadManager.Instance.LoadAudioMixer(AssetPath.kSEAudioMixer);
        }

        public static AudioSource Play(string assetName, bool isLoop = false, float duration = 0f, CancellationTokenSource tokenSource = null)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            // 同じフレームで同じSEが鳴る数を制限してキューに登録
            if (m_Queue.Count(x => x.m_AssetName == assetName) < kSameSECountMax)
            {
                RequestData requestData = new RequestData(){ 
                    m_AssetName=assetName, 
                    m_IsLoop=isLoop, 
                    m_Duration=duration,
                    m_TargetAudioSouce=GetAudioSource() 
                };
                m_Queue.Add( requestData );
                return requestData.m_TargetAudioSouce;
            }
            return null;
        }

        private static async UniTask PlayStart(string assetName, bool isLoop, float volume, float duration, AudioSource audioSource, CancellationTokenSource tokenSouce = null)
        {
            if(audioSource == null)
            {
                return;
            }

            AudioClip clip;

            if (tokenSouce == null)
                tokenSouce = new CancellationTokenSource();

            clip = await AssetLoadManager.Instance.LoadAudioAsync(AssetPath.GetSoundFileName(assetName), audioSource.gameObject.GetCancellationTokenOnDestroy());

            if (clip != null)
            {
                if (duration > 0)
                {
                    await UniTask.Delay((int)(duration * 1000), cancellationToken: tokenSouce.Token);
                }

                audioSource.clip = clip;
                audioSource.time = 0;
                audioSource.volume = volume;
                audioSource.loop = isLoop;
                audioSource.Play();
                audioSource.name = isLoop ? "Loop audio" : "One shot audio";

                // OneShotは終了待ちをする
                if( !isLoop ) {
                    try
                    {
                        await UniTask.WaitWhile(() =>
                        {
                            // Unity再生停止時にエラーが出るのでとりあえずのnullチェック
                            if (audioSource != null)
                            {
                                return audioSource.isPlaying;
                            }

                            return false;
                        }, cancellationToken: tokenSouce.Token);
                    }
                    finally
                    {
                        // Unity再生停止時にエラーが出るのでとりあえずのnullチェック
                        if (audioSource != null)
                        {
                            audioSource.Stop();
                            audioSource.clip = null;
                        }
                    }
                }
            }

            // Unity再生停止時にエラーが出るのでとりあえずのnullチェック
            if( !isLoop ) {
                if (audioSource != null)
                {
                    audioSource.gameObject.SetActive(false);
                }
            }
        }

        private static AudioSource GetAudioSource()
        {
            AudioSource audioSource;

            if (AudioSources.Any(x => !x.gameObject.activeSelf))
            {
                audioSource = AudioSources.First(x => !x.gameObject.activeSelf);
                audioSource.gameObject.SetActive(true);
            }
            else
            {
                if (AudioSources.Count >= kAudioSourceCountMax)
                {
                    return null;
                }

                var gameObject = new GameObject("One shot audio");
                audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
                audioSource.outputAudioMixerGroup = AudioMixer.FindMatchingGroups("Master")[0];
                AudioSources.Add(audioSource);
                GameObject.DontDestroyOnLoad(gameObject);
            }

            return audioSource;
        }

        public static void StopAllSE()
        {
            foreach (var audioSource in AudioSources)
            {
                if (audioSource.gameObject.activeSelf)
                {
                    audioSource.gameObject.SetActive(false);
                }
            }
        }

        public static void Stop( AudioSource target )
        {
            if (target.gameObject.activeSelf)
            {
                target.Stop();
                target.clip = null;
                target.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 全てのSEをメモリに常駐させておく
        /// </summary>
        public static void HoldLoadAllSE()
        {
            HoldAssetLoadManager.Instance.LoadAssetsFromLabel<AudioClip>("Label-SE");
        }
    }
}

