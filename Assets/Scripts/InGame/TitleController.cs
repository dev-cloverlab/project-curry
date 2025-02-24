using curry.Common;
using curry.InGame;
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
        private GameController m_GameController;

        private StateMachine m_StateMachine = new ();

        private void Awake()
        {
            UnityUtility.SetActive(m_UICanvas, true);
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
            ToInGameTask().Forget();
        }

        private void RankingProcess(StateMachineProcess _)
        {
            DebugLogWrapper.Log($"<color=white> [[Debug]] : RankingProcess </color>");
        }

        private void SettingProcess(StateMachineProcess _)
        {
            DebugLogWrapper.Log($"<color=white> [[Debug]] : SettingProcess </color>");
        }

        private void ExitProcess(StateMachineProcess _)
        {
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
