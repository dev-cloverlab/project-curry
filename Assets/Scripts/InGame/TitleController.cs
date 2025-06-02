using curry.Common;
using curry.InGame;
using curry.leaderboard;
using curry.Ranking;
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
            Leaderboard,
            Setting,
            Credits,
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
        private CreditsUI m_CreditsUI;
        [SerializeField]
        private GameController m_GameController;
        [SerializeField]
        private Leaderboard m_Leaderboard;


        private StateMachine m_StateMachine = new ();

        private bool m_IsFirstSetting = true;
        private bool m_IsFirstCredits = true;
        private bool m_IsFirstLeaderboard = true;

        private void Awake()
        {
            UnityUtility.SetActive(m_UICanvas, true);
            UnityUtility.SetActive(m_SettingUI, false);
            UnityUtility.SetActive(m_Leaderboard, false);
            UnityUtility.SetActive(m_CreditsUI, false);
            SetState();
        }


#region SetState

        private void SetState()
        {
            m_StateMachine.AddState((int)State.Init, InitProcess);
            m_StateMachine.AddState((int)State.Title, null);
            m_StateMachine.AddState((int)State.ToInGame, ToInGameProcess);
            m_StateMachine.AddState((int)State.Leaderboard, LeaderboardProcess);
            m_StateMachine.AddState((int)State.Setting, SettingProcess);
            m_StateMachine.AddState((int)State.Credits, CreditsProcess);
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

        private void LeaderboardProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();
            LeaderboardTask().Forget();
        }

        private void SettingProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();
            SettingTask().Forget();
        }

        private void CreditsProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();
            CreaditsTask().Forget();
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
            await UniTask.WaitUntil(() => RuntimeInitialize.IsSetup);

            EnvSoundManager.Instance.Player.PlayLoop("env_forest_day");

            m_TitleUI.GameStartAction = OnPressGameStart;
            m_TitleUI.RankingAction = OnPressLeaderboard;
            m_TitleUI.SettingAction = OnPressSetting;
            m_TitleUI.CreditsAction = OnPressCredits;
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

        private async UniTask CreaditsTask()
        {
            UnityUtility.SetActive(m_CreditsUI, true);

            if (m_IsFirstCredits)
            {
                m_CreditsUI.OpenedEvent.AddListener(() =>
                {
                    m_CreditsUI.Activate(true);
                });

                m_CreditsUI.ClosedEvent.AddListener(() =>
                {
                    UnityUtility.SetActive(m_CreditsUI, false);
                    m_StateMachine.NextState((int)State.Title);
                });

                m_IsFirstCredits = false;
            }

            m_CreditsUI.OpenAnimation();
        }

        private async UniTask LeaderboardTask()
        {
            // リーダーボード更新
            await CurryLeaderBoardManager.Instance.SetupLeaderboardDataList();

            UnityUtility.SetActive(m_Leaderboard, true);

            if (m_IsFirstLeaderboard)
            {
                m_Leaderboard.OpenedEvent.AddListener(() =>
                {
                    m_Leaderboard.Activate(true);
                });

                m_Leaderboard.ClosedEvent.AddListener(() =>
                {
                    UnityUtility.SetActive(m_Leaderboard, false);
                    m_StateMachine.NextState((int)State.Title);
                });

                m_Leaderboard.Setup(CurryLeaderBoardManager.Instance.GlobalDataList,
                    CurryLeaderBoardManager.Instance.GlobalAroundUserDataList,
                    CurryLeaderBoardManager.Instance.FriendDataList);

                m_IsFirstLeaderboard = false;
            }

            m_Leaderboard.OpenAnimation();
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

        private void OnPressLeaderboard()
        {
            if (!m_StateMachine.IsState((int)State.Title))
            {
                return;
            }

            m_StateMachine.NextState((int)State.Leaderboard);
        }

        private void OnPressSetting()
        {
            if (!m_StateMachine.IsState((int)State.Title))
            {
                return;
            }

            m_StateMachine.NextState((int)State.Setting);
        }

        private void OnPressCredits()
        {
            if (!m_StateMachine.IsState((int)State.Title))
            {
                return;
            }

            m_StateMachine.NextState((int)State.Credits);
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
