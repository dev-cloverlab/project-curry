using System;
using curry.UI;
using curry.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace curry.InGame
{
    public class InGameUI : UIBase
    {
        [SerializeField]
        private GameObject m_ClickObject;

        [SerializeField]
        private CanvasGroup m_MainUICanvasGroup;

        [SerializeField]
        private CanvasGroup m_GameOverCanvasGroup;

        [SerializeField]
        private Slider m_GameOverSlider;

        [SerializeField]
        private TextMeshProUGUI m_ScoreText;

        public void Init()
        {
            m_MainUICanvasGroup.alpha = 0;
            m_GameOverCanvasGroup.alpha = 0;
            SetScore(0);
            SetActiveClickObject(false);
        }

        public void SetActiveClickObject(bool active)
        {
            UnityUtility.SetActive(m_ClickObject, active);
        }

        public async UniTask FadeIn()
        {
            await m_MainUICanvasGroup.DOFade(1f, 1.0f);
        }

        public async UniTask FadeInGameOver()
        {
            await m_GameOverCanvasGroup.DOFade(1f, 1.0f);
        }

        public void SetGauge(decimal now, decimal max)
        {
            m_GameOverSlider.value = (float)((max - now) / max);
        }

        public void SetScore(long score)
        {
            var timeSpan = TimeSpan.FromSeconds(score);

            var daySuffix = timeSpan.Days > 1 ? "days" : "day";
            var day = timeSpan.Days > 0 ? $"{timeSpan.Days}{daySuffix} " : string.Empty;

            var hour = $"{timeSpan.Hours:00}:";

            var minutes = $"{timeSpan.Minutes:00}:";

            var seconds = $"{timeSpan.Seconds:00}";

            m_ScoreText.SetText($"{day}{hour}{minutes}{seconds}");
        }
    }
}
