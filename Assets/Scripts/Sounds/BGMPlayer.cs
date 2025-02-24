using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using curry.Common;
using UnityEngine.AddressableAssets;

namespace curry.Sound
{

    public static class BgmPlayer
    {
        public static string CurrentName { private set; get; } = "";
        public const int SplitMax = 20;
        public const float MaxVolume = 0.2f;
        private static float currentVolume = MaxVolume;
        static AudioSource bgmAudioSource;
        private static CancellationTokenSource cancellationTokenSource;

        public static void Init()
        {
            if (bgmAudioSource == null)
            {
                bgmAudioSource = new GameObject("BGMIntroAudioSource").AddComponent<AudioSource>();
                bgmAudioSource.spatialBlend = 0f;
                GameObject.DontDestroyOnLoad(bgmAudioSource);
            }
        }

        public static async UniTask Play(string assetName, bool isLoop = true, float fadeDuration = 0)
        {
            if (string.IsNullOrEmpty(assetName) || CurrentName == assetName) return;

            // 別のループBGM再生待ちがあればキャンセルしておきます
            Cancel();

            cancellationTokenSource = new CancellationTokenSource();
            CurrentName = assetName;

            var clip = await AssetLoadManager.Instance.LoadAudioAsync(AssetPath.GetSoundFileName(assetName), bgmAudioSource!.gameObject.GetCancellationTokenOnDestroy());

            bgmAudioSource.clip = clip;

            // 一度処理を通さないと重い
            bgmAudioSource.Play();
            bgmAudioSource.Stop();

            bgmAudioSource.loop = isLoop;

            bgmAudioSource!.Play();
            cancellationTokenSource = null;

            FadeIn(fadeDuration);
        }

        public static void Stop()
        {
            if (bgmAudioSource != null)
            {
                bgmAudioSource.Stop();
            }
            CurrentName = "";

            Cancel();
        }

        /// <summary>
        /// UniTaskのキャンセルを行います
        /// </summary>
        private static void Cancel()
        {
            if (cancellationTokenSource != null)
            {
               cancellationTokenSource.Cancel();
               cancellationTokenSource = null;
            }
        }

        public static void FadeOut(float duration = 1.0f)
        {
            bgmAudioSource.DOFade(0, duration);
        }
        public static void FadeIn(float duration = 1.0f)
        {
            bgmAudioSource.DOFade(currentVolume, duration);
        }

        public static void VolumeSet(int i)
        {
            currentVolume = MaxVolume * (i / (float)SplitMax);
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = currentVolume;
            }
        }
    }
}

