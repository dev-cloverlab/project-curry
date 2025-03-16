using curry.Common;
using curry.InGame;
using curry.Sound;
using curry.UI;
using curry.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace curry.Title
{
    public class TitleController : MonoBehaviour
    {
        private enum State
        {
            Init,
            Title,
            ToInGame,
            Ranking,
            Setting,
            Exit,
        }

        [SerializeField]
        private GameObject m_UICanvas;

        [SerializeField]
        private Transform m_WindowParent;
        [SerializeField]
        private TitleUI m_TitleUI;
        [SerializeField]
        private SettingUI m_SettingUI;
        [SerializeField]
        private GameController m_GameController;

        private StateMachine m_StateMachine = new ();

        private bool m_IsFirstSetting = true;

        private void Awake()
        {
            UnityUtility.SetActive(m_UICanvas, true);
            UnityUtility.SetActive(m_SettingUI, false);
            SetState();
        }


#region SetState

        private void SetState()
        {
            m_StateMachine.AddState((int)State.Init, InitProcess);
            m_StateMachine.AddState((int)State.Title, null);
            m_StateMachine.AddState((int)State.ToInGame, ToInGameProcess);
            m_StateMachine.AddState((int)State.Ranking, RankingProcess);
            m_StateMachine.AddState((int)State.Setting, SettingProcess);
            m_StateMachine.AddState((int)State.Exit, ExitProcess);

            m_StateMachine.NextState((int)State.Init);
        }

#endregion

#region StateProcess

        private void InitProcess(StateMachineProcess _)
        {
            InitTask().Forget();
        }

        private void ToInGameProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();
            ToInGameTask().Forget();
        }

        private void RankingProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();
            DebugLogWrapper.Log($"<color=white> [[Debug]] : RankingProcess </color>");
        }

        private void SettingProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();
            SettingTask().Forget();
        }

        private void ExitProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

#endregion

#region Tasks

        private async UniTask InitTask()
        {
            EnvSoundManager.Instance.Player.PlayLoop("env_forest_day");

            m_TitleUI.GameStartAction = OnPressGameStart;
            m_TitleUI.RankingAction = OnPressRanking;
            m_TitleUI.SettingAction = OnPressSetting;
            m_TitleUI.ExitAction = OnPressExit;

            await UniTask.WaitForSeconds(2.0f);

            await FaderManager.Instance.FadeIn();

            m_StateMachine.NextState((int)State.Title);
        }

        private async UniTask ToInGameTask()
        {
            await m_TitleUI.FadeOut();

            UnityUtility.SetActive(m_TitleUI, false);
            m_GameController.GameStart();
        }

        private async UniTask SettingTask()
        {
            UnityUtility.SetActive(m_SettingUI, true);

            if (m_IsFirstSetting)
            {
                m_SettingUI.OpenedEvent.AddListener(() =>
                {
                    m_SettingUI.Activate(true);
                });

                m_SettingUI.ClosedEvent.AddListener(() =>
                {
                    UnityUtility.SetActive(m_SettingUI, false);
                    m_StateMachine.NextState((int)State.Title);
                });
                m_IsFirstSetting = false;
            }

            m_SettingUI.Setup();
            m_SettingUI.OpenAnimation();
        }

#endregion

#region TitleButtonActions

        private void OnPressGameStart()
        {
            if (!m_StateMachine.IsState((int)State.Title))
            {
                return;
            }

            m_StateMachine.NextState((int)State.ToInGame);
        }

        private void OnPressRanking()
        {
            if (!m_StateMachine.IsState((int)State.Title))
            {
                return;
            }

            m_StateMachine.NextState((int)State.Ranking);
        }

        private void OnPressSetting()
        {
            if (!m_StateMachine.IsState((int)State.Title))
            {
                return;
            }

            m_StateMachine.NextState((int)State.Setting);
        }

        private void OnPressExit()
        {
            if (!m_StateMachine.IsState((int)State.Title))
            {
                return;
            }

            m_StateMachine.NextState((int)State.Exit);
        }

#endregion
    }
}
