using System;
using curry.Common;
using curry.Ranking;
using curry.Sound;
using curry.UI;
using curry.Utilities;
using Cysharp.Threading.Tasks;
using Fluxy;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cursor = UnityEngine.Cursor;

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

        [SerializeField]
        private FluxyTarget[] m_Bubbles;

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
        private bool m_DisplayLightMessage;
        private bool m_ClosedLightMessage;

        private const float kBubbleForceNone = 0f;
        private const float kBubbleForceMin = 0.1f;
        private const float kBubbleForceMax = 1.2f;
        // float m_FpsDeltaTime = 0.0f;
        // float m_FpsDisplay = 0.0f;
        // float m_FpsTimer = 0.0f;

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
            SetBubbleForce(kBubbleForceNone);
            UnityUtility.SetActive(m_FireObject, false);
            SetState();

            m_RadiusSqr = LadleRadius * LadleRadius;
            m_DisplayLightMessage = false;
            m_ClosedLightMessage = false;
            m_LightController.LightMessageAction = DisplayLightMessage;
            m_LightController.LightMessageCloseAction = CloseLightMessage;
        }

        private void DisplayLightMessage()
        {
            if (m_DisplayLightMessage)
            {
                return;
            }

            m_InGameUI.SetActiveLightMessage(true).Forget();

            m_DisplayLightMessage = true;
        }

        private void CloseLightMessage()
        {
            if (!m_DisplayLightMessage || m_ClosedLightMessage)
            {
                return;
            }

            m_InGameUI.SetActiveLightMessage(false).Forget();

            m_ClosedLightMessage = true;
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
            // m_FpsDeltaTime += (Time.unscaledDeltaTime - m_FpsDeltaTime) * 0.1f;
            // m_FpsTimer += Time.unscaledDeltaTime;
            // if (m_FpsTimer >= 0.5f)
            // {
            //     m_FpsDisplay = 1.0f / m_FpsDeltaTime;
            //     m_FpsTimer = 0.0f;
            // }

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

        private float m_ColorValMin = 1.0f;

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

            var bubbleForce = kBubbleForceMin + ratio * (kBubbleForceMax - kBubbleForceMin);
            SetBubbleForce(bubbleForce);

            colorVal = Mathf.Clamp(colorVal, 0f, 1f);

            if (colorVal <= m_ColorValMin)
            {
                m_ColorValMin = colorVal;
            }
            else
            {
                colorVal = m_ColorValMin;
            }

            var isActiveSteam = ratio > 0.5f && ratio < 0.9f;

            UnityUtility.SetActive(m_Steam, isActiveSteam);

            if (isActiveSteam)
            {
                var countBase = (ratio-0.49f) * 10;
                var maxParticles = Mathf.RoundToInt(countBase * countBase * 10);
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
            SetBubbleForce(kBubbleForceMin);
            m_LightController.SetStart();
        }

        private void SetBubbleForce(float force)
        {
            float scale;

            if (force <= 0.6f)
            {
                scale = 0.3f;
            }
            else if (force <= 1.2f)
            {
                // 線形補間: force値0.6fから1.2fの範囲をscale 0.3fから0.7fに対応させる
                scale = 0.3f + (force - 0.6f) * (0.7f - 0.3f) / (1.2f - 0.6f);
            }
            else
            {
                // 範囲外の値は最大値に制限する
                scale = 0.7f;
            }

            foreach (var bubble in m_Bubbles)
            {
                bubble.force.x = force;
                bubble.scale.x = scale;
                bubble.scale.y = scale;
            }
        }

        //
        // void OnGUI()
        // {
        //     GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + m_FpsDisplay.ToString("F2"));
        // }
    }
}
