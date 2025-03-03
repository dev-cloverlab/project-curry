using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace curry.Sound
{
    public class BgmPlayer : MonoBehaviour
    {
        [System.Serializable]
        protected class AudioClipInfo
        {
            [SerializeField]
            private string m_Name;
            public string Name => m_Name;

            [SerializeField]
            private AudioClip m_AudioClip;
            public AudioClip AudioClip => m_AudioClip;
        }

        [SerializeField]
        protected List<AudioSource> m_BgmAudioSource;

        [SerializeField]
        protected List<AudioClipInfo> m_BgmAudioInfoList;

        public static string CurrentName { get; private set; }
        protected static float m_CurrentVolume = BgmManager.MaxVolume;
        private int m_CurrentIdx = -1;

        private Coroutine m_BgmCoroutine;

        private bool m_LoopFlg;
        private bool m_IsLoopFade;
        private string m_LoopName;
        public string LoopName => m_LoopName;
        private float m_LoopLength;
        private float m_Timer;
        private const float kLoopDuration = 3.0f;
        private List<UniTask> m_TaskList = new();

        public void SetLoopName(string name)
        {
            m_LoopName = name;
        }

        public void PlayLoop(string name, float duration = 0.0f)
        {
            var audioClip = m_BgmAudioInfoList.FirstOrDefault(x => x.Name == name)?.AudioClip;
            if (audioClip == null)
            {
                DebugLogWrapper.LogAssertion($"Audio clip not found: {name}");
                return;
            }

            Play(name, 0).Forget();

            m_LoopName = name;
            m_LoopLength = audioClip.length;
            m_Timer = 0.0f;
            m_LoopFlg = true;
            m_IsLoopFade = false;
        }

        private void Update()
        {
            UpdateLoop();
        }

        private void UpdateLoop()
        {
            if (!m_LoopFlg)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            m_Timer += deltaTime;

            if (m_Timer + 5.0f +  kLoopDuration >= m_LoopLength)
            {
                m_Timer = 0;

                m_IsLoopFade = true;
                Play(m_LoopName, kLoopDuration, () => { m_IsLoopFade = false; }).Forget();
            }
        }

        public async UniTask Play(string name, float duration, UnityAction onComplete = null)
        {
            var audioClip = m_BgmAudioInfoList.FirstOrDefault(x => x.Name == name)?.AudioClip;
            if (audioClip == null)
            {
                DebugLogWrapper.LogAssertion($"Audio clip not found: {name}");
                return;
            }

            CurrentName = name;
            var prevIdx = 0;
            var idx = 0;

            if (m_CurrentIdx != 0)
            {
                prevIdx = 1;
            }
            else
            {
                idx = 1;
            }

            m_CurrentIdx = idx;
            var currentSource = m_BgmAudioSource[idx];
            var prevSource = m_BgmAudioSource[prevIdx];

            currentSource.clip = audioClip;
            m_BgmAudioSource[idx].Play();

            m_TaskList.Clear();
            m_TaskList.Add(FadeIn(currentSource, duration));
            m_TaskList.Add(FadeOut(prevSource, duration));

            await m_TaskList;
            prevSource.Stop();

            onComplete?.Invoke();
        }

        private AudioSource GetCurrentSource()
        {
            return m_BgmAudioSource[m_CurrentIdx];
        }

        private AudioSource GetOtherSource()
        {
            var idx = m_CurrentIdx == 0 ? 1 : 0;
            return m_BgmAudioSource[idx];
        }

        public void Stop(float duration)
        {
            var source = GetCurrentSource();
            FadeOut(source, duration).Forget();
            var otherSource = GetOtherSource();
            otherSource.Stop();
        }

        public async UniTask StopLoop(float duration)
        {
            m_LoopFlg = false;
            await UniTask.WaitWhile(() => m_IsLoopFade);
            Stop(duration);
        }

        public void VolumeSet(int volume)
        {
            m_CurrentVolume = BgmManager.MaxVolume * (volume / (float)BgmManager.SplitMax);
            foreach (var source in m_BgmAudioSource)
            {
                if (source != null)
                {
                    source.volume = m_CurrentVolume;
                }
            }
        }

        private async UniTask FadeOut(AudioSource source, float duration = 1.0f)
        {
            await source.DOFade(0, duration).SetEase(Ease.InQuad);
        }

        private async UniTask FadeIn(AudioSource source, float duration = 1.0f)
        {
            await source.DOFade(m_CurrentVolume, duration).SetEase(Ease.OutQuad);
        }
    }
}
