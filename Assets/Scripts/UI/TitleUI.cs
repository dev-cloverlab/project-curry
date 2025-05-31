using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace curry.UI
{
    public class TitleUI : UIBase
    {
        [SerializeField]
        private CanvasGroup m_ButtonsCanvasGroup;

        [SerializeField]
        private CanvasGroup m_BackGroundCanvasGroup;

        public UnityAction GameStartAction { get; set; }
        public UnityAction RankingAction { get; set; }
        public UnityAction SettingAction { get; set; }
        public UnityAction CreditsAction { get; set; }
        public UnityAction ExitAction { get; set; }

        public async UniTask FadeOut()
        {
            await m_ButtonsCanvasGroup.DOFade(0f, 1.0f);
            await m_BackGroundCanvasGroup.DOFade(0f, 1.0f);
        }

#region Unity Inspector

        public void OnPressGameStart()
        {
            GameStartAction?.Invoke();
        }

        public void OnPressRanking()
        {
            RankingAction?.Invoke();
        }

        public void OnPressSetting()
        {
            SettingAction?.Invoke();
        }

        public void OnPressCredits()
        {
            CreditsAction?.Invoke();
        }

        public void OnPressExit()
        {
            ExitAction?.Invoke();
        }

#endregion
    }
}
