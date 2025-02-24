using curry.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace curry.InGame
{
    public class GameController : MonoBehaviour
    {
        private enum State
        {
            TitleWait,
            ClickWait,
            Idle,
            GameOverFade,
            GameOver,
            AfterGameOver,
        }

        private StateMachine m_StateMachine = new ();

        [SerializeField]
        private InGameUI m_InGameUI;

        [SerializeField]
        private LadleAngleController m_LadleAngleController;

        private Vector2 m_MouseAxis = Vector2.zero;

        private Protector<bool> m_IsStart;
        private Protector<bool> m_IsMove;
        private Protector<decimal> m_NowGauge;
        private Protector<long> m_Score;
        private float m_TempTime;

        private void Awake()
        {
            m_InGameUI.Init();
            SetState();
        }

#region SetState

        private void SetState()
        {
            m_StateMachine.AddState((int)State.TitleWait, null);
            m_StateMachine.AddState((int)State.ClickWait, ClickWaitProcess);
            m_StateMachine.AddState((int)State.Idle, IdleProcess);
            m_StateMachine.AddState((int)State.GameOverFade, GameOverFadeProcess);
            m_StateMachine.AddState((int)State.GameOver, null);
            m_StateMachine.AddState((int)State.AfterGameOver, AfterGameOverProcess);

            m_StateMachine.NextState((int)State.TitleWait);
        }

#endregion

#region StateProcess

        private void ClickWaitProcess(StateMachineProcess _)
        {
            EnableCursor(true);
            m_InGameUI.SetActiveClickObject(true);
        }

        private void IdleProcess(StateMachineProcess _)
        {
            m_IsStart = true;

            EnableCursor(false);
            m_InGameUI.SetActiveClickObject(false);
        }

        private void GameOverFadeProcess(StateMachineProcess _)
        {
            EnableCursor(true);
            GameOverFadeTask().Forget();
        }

        private void AfterGameOverProcess(StateMachineProcess _)
        {
            AfterGameOverTask().Forget();
        }

#endregion

#region Tasks

        private async UniTask GameOverFadeTask()
        {
            await m_InGameUI.FadeInGameOver();
            m_StateMachine.NextState((int)State.GameOver);
        }

        private async UniTask AfterGameOverTask()
        {
            await FaderManager.Instance.FadeOut(2.0f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

#endregion

        private void Update()
        {
            if (m_StateMachine.IsState((int)State.GameOver))
            {
                if (Input.GetMouseButtonDown(0) ||
                    Input.GetKeyDown(KeyCode.Escape))
                {
                    m_StateMachine.NextState((int)State.AfterGameOver);
                }

                return;
            }

            if (m_StateMachine.IsState((int)State.GameOverFade) ||
                m_StateMachine.IsState((int)State.AfterGameOver))
            {
                return;
            }

            var deltaTime = Time.deltaTime;

            UpdateInput();

            if (m_IsStart)
            {
                UpdateGameOverGauge(deltaTime);
            }
        }

        private void LateUpdate()
        {
            m_IsMove = false;
        }

        private void UpdateGameOverGauge(float deltaTime)
        {
            if (!m_IsMove)
            {
                m_NowGauge += (decimal)deltaTime;
            }
            else
            {
                m_NowGauge -= (decimal)deltaTime * GameSetting.qHpCDMzyjVtgsAHu;
            }

            m_InGameUI.SetGauge(m_NowGauge, GameSetting.wiyxJiGVidnUSaqY);

            if (m_NowGauge >= GameSetting.wiyxJiGVidnUSaqY)
            {
                m_StateMachine.NextState((int)State.GameOverFade);
                return;
            }

            // 1秒で1スコア
            m_TempTime += deltaTime;
            if (m_TempTime >= 1.0f)
            {
                m_TempTime -= 1.0f;
                m_Score += 1;
                m_InGameUI.SetScore(m_Score);
            }
        }

        private void UpdateInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (m_StateMachine.IsState((int)State.ClickWait))
                {
                    m_StateMachine.NextState((int)State.Idle);
                }
                else if (m_StateMachine.IsState((int)State.Idle))
                {
                    m_StateMachine.NextState((int)State.ClickWait);
                }
            }

            if (m_StateMachine.IsState((int)State.Idle))
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    m_StateMachine.NextState((int)State.ClickWait);
                    return;
                }

                UpdateIdle();
            }
        }

        private void UpdateIdle()
        {
            m_MouseAxis.x = Input.GetAxis("Mouse X");
            m_MouseAxis.y = Input.GetAxis("Mouse Y");

            var magnitude = m_MouseAxis.magnitude;

            if (magnitude < GameSetting.AFBqBqiDtCdWeXvD)
            {
                return;
            }

            m_IsMove = true;
            m_LadleAngleController.LadleRotate(magnitude);
        }

        public void GameStart()
        {
            GameStartAsync().Forget();
        }

        private async UniTask GameStartAsync()
        {
            await m_InGameUI.FadeIn();
            m_StateMachine.NextState((int)State.ClickWait);
        }

        private void EnableCursor(bool enable)
        {
            // カーソル表示
            Cursor.visible = enable;
            // カーソルを自由に動かせる
            Cursor.lockState = enable ? CursorLockMode.None: CursorLockMode.Confined;
        }
    }
}
