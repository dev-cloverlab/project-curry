using System;
using curry.Common;
using curry.Ranking;
using curry.Sound;
using curry.UI;
using curry.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace curry.InGame
{
    public class GameController : MonoBehaviour
    {
        private enum State
        {
            Init,
            TitleWait,
            ClickWait,
            Idle,
            GameOverFade,
            GameOver,
            AfterGameOver,
            Setting,
        }

        private StateMachine m_StateMachine = new ();

        [SerializeField]
        private InGameUI m_InGameUI;

        [SerializeField]
        private LadleAngleController m_LadleAngleController;

        [SerializeField]
        private Transform m_LadleEntityTransform;

        [SerializeField]
        private GameObject m_FireObject;

        [SerializeField]
        private LightController m_LightController;

        [SerializeField]
        private Rigidbody m_LadleRigidBody;

        [SerializeField]
        private SettingUI m_SettingUI;

        [SerializeField]
        private MeshRenderer m_MeshRenderer;

        [SerializeField]
        private GameObject m_Steam;

        [SerializeField]
        private ParticleSystem m_SteamParticle;

        [SerializeField]
        private GameObject m_KogeSteam;

        private Vector2 m_MouseAxis = Vector2.zero;

        private Protector<bool> m_IsStart;
        private Protector<bool> m_IsMove;
        private Protector<decimal> m_NowGauge;
        private Protector<int> m_Score;
        private float m_TempTime;

        private bool m_IsFirstStart = true;

        private const float LadleRadius = 1.1f;
        private float m_RadiusSqr;
        private bool m_IsFirstSetting = true;
        private static readonly int kAlbedoTintID = Shader.PropertyToID("Color_3D2DB61D");
        private Material m_CurryMat;
        private Color m_CurryColor = new (1.0f, 1.0f, 1.0f, 1.0f);
        private ParticleSystem.MainModule m_SteamParticleMain;

        private void Awake()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
            m_CurryMat = m_MeshRenderer.materials[0];
            UnityUtility.SetActive(m_Steam, false);
            UnityUtility.SetActive(m_KogeSteam, false);
            m_SteamParticleMain = m_SteamParticle.main;

            m_InGameUI.Init();
            m_InGameUI.OnClickSetting = () =>
            {
                if (!m_StateMachine.IsState((int)State.ClickWait))
                {
                    return;
                }

                m_StateMachine.NextState((int)State.Setting);
            };
            UnityUtility.SetActive(m_FireObject, false);
            SetState();

            m_RadiusSqr = LadleRadius * LadleRadius;
        }

        private void OnDestroy()
        {
            if (m_CurryMat != null)
            {
                Destroy(m_CurryMat);
                m_CurryMat = null;
            }
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
            m_StateMachine.AddState((int)State.Setting, SettingProcess);

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
            if(m_SettingUI.IsActivate)
                m_SettingUI.Close();
            EnableCursor(true);
            GameOverFadeTask().Forget();
        }

        private void AfterGameOverProcess(StateMachineProcess _)
        {
            AfterGameOverTask().Forget();
        }

        private void SettingProcess(StateMachineProcess _)
        {
            SEPlayer.PlaySelectSE();
            SettingTask().Forget();
        }

#endregion

#region Tasks

        private async UniTask GameOverFadeTask()
        {
            await m_InGameUI.FadeInGameOver();
            await m_InGameUI.SetScoreCanvasGroupAlpha(true, 1.0f);
            m_StateMachine.NextState((int)State.GameOver);
        }

        private async UniTask AfterGameOverTask()
        {
            EnvSoundManager.Instance.Player.StopLoop(2.0f);
            FireSoundManager.Instance.Player.StopLoop(2.0f);
            await FaderManager.Instance.FadeOut(2.0f);
            await CurryLeaderBoardManager.Instance.UploadScore(m_Score);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

                    if (m_StateMachine.IsState((int)State.Setting))
                    {
                        m_StateMachine.NextState((int)State.ClickWait);
                    }
                });
                m_IsFirstSetting = false;
            }

            m_SettingUI.Setup();
            m_SettingUI.OpenAnimation();
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

            if (m_NowGauge < 0)
            {
                m_NowGauge = 0;
            }

            var ratio = (float)(m_NowGauge / GameSetting.wiyxJiGVidnUSaqY);

            float colorVal;

            if (ratio <= 0.5f)
            {
                colorVal = 1.0f;
            }
            else
            {
                colorVal = (1.0f - ratio) * 2;
            }

            colorVal = Mathf.Clamp(colorVal, 0f, 1f);

            var isActiveSteam = ratio > 0.5f && ratio < 0.9f;

            UnityUtility.SetActive(m_Steam, isActiveSteam);

            if (isActiveSteam)
            {
                var countBase = (ratio-0.49f) * 10;
                var maxParticles = Mathf.RoundToInt(countBase * countBase * 10);
                DebugLogWrapper.Log($"<color=white> [[Debug]] : maxParticles {maxParticles} </color>");
                m_SteamParticleMain.maxParticles = maxParticles;
            }

            UnityUtility.SetActive(m_KogeSteam, ratio >= 0.9f);

            m_CurryColor.r = colorVal;
            m_CurryColor.g = colorVal;
            m_CurryColor.b = colorVal;

            m_CurryMat.SetColor(kAlbedoTintID, m_CurryColor);

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
                    // 設定ボタンの上にカーソルがある場合はクリック待機が続く
                    if (m_InGameUI.IsSettingButtonHover())
                    {
                        return;
                    }

                    if (m_IsFirstStart)
                    {
                        OnFirstStart();

                        m_IsFirstStart = false;
                    }

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

                UpdateLadleMoveAdulation();
            }
        }

        // マウスが動いていたら、おたまは追従せずに回転する
        // private void UpdateLadleMove()
        // {
        //     float x;
        //     float y;
        //     m_MouseAxis.x = x = Input.GetAxis("Mouse X");
        //     m_MouseAxis.y = y = Input.GetAxis("Mouse Y");
        //
        //     var moveSqr = x * x + y * y;
        //
        //     if (moveSqr < GameSetting.AFBqBqiDtCdWeXvD )
        //     {
        //         return;
        //     }
        //
        //     m_LadleAngleController.LadleRotate(Mathf.Sqrt(moveSqr));
        //     m_IsMove = true;
        // }

        // マウスが動いたらその動きに追従して動く
        private void UpdateLadleMoveAdulation()
        {
            float x;
            float y;
            m_MouseAxis.x = x = Input.GetAxis("Mouse X");
            m_MouseAxis.y = y = Input.GetAxis("Mouse Y");

            var moveSqr = x * x + y * y;

            if (moveSqr < GameSetting.AFBqBqiDtCdWeXvD )
            {
                return;
            }

            var pos = m_LadleRigidBody.position;
            pos.x += m_MouseAxis.x * GameSetting.qgUjmtKLExaPGUTJ / 10;
            pos.z += m_MouseAxis.y * GameSetting.qgUjmtKLExaPGUTJ / 10;

            // 現在位置の x, z を円の中心からの相対座標として取得
            var offsetX = pos.x;
            var offsetZ = pos.z;

            // 半径をの二乗計算
            var distanceSqr = offsetX * offsetX + offsetZ * offsetZ;

            // 半径が制限を超えている場合のみ修正
            if (distanceSqr > m_RadiusSqr)
            {
                // 角度を求める
                float angle = Mathf.Atan2(offsetZ, offsetX);

                // 半径を制限して x, z を計算
                pos.x = Mathf.Cos(angle) * LadleRadius;
                pos.z = Mathf.Sin(angle) * LadleRadius;
            }

            m_LadleRigidBody.MovePosition(pos);
            m_IsMove = true;
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

        private void OnFirstStart()
        {
            FireSoundManager.Instance.Player.PlayLoop("fire");
            UnityUtility.SetActive(m_FireObject, true);
            m_LightController.SetStart();
        }
    }
}
