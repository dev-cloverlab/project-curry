using curry.Sound;
using curry.Utilities;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace curry.InGame
{
    public class LightController : MonoBehaviour
    {
        [SerializeField]
        private Transform m_LightParentTransform;

        [SerializeField]
        private Light m_SunLight;

        [SerializeField]
        private GameObject m_LampLightObj;

        [SerializeField]
        private InGameBirdsController m_BirdsController;

        private bool m_IsStart;
        private float m_Timer;
        private const float kMaxRoatationAngle = 360.0f;
        private float m_Intensity = 2f;
        private Color32 m_DefaultColor;
        private const float kMinGreen = 152;
        private const float kMinBlue = 80;
        private float m_BeforeAngle;
        private const float kShadowStrengthMax = 0.8f;
        private const float kShadowStrengthMin = 0f;
        private float m_Angle;
        private bool m_IsFirstBird = true;

        private void Awake()
        {
            m_DefaultColor = m_SunLight.color;
            UnityUtility.SetActive(m_LampLightObj, false);
        }

        public void SetStart()
        {
            m_IsStart = true;
        }

        private void Update()
        {
            if (!m_IsStart)
            {
                return;
            }

            float multipilar;

            if (m_BeforeAngle >= 120 && m_BeforeAngle <= 240)
            {
                multipilar = 2f;
            }
            else
            {
                multipilar = 1/2f;
            }

            var deltaTime = Time.deltaTime * multipilar;
            m_Timer += deltaTime;

            // 元の時間進行比率
            var totalDuration = GameSetting.XdYpLzQaWmNbVoTc; // 3分
            var ratio = m_Timer / totalDuration;

            var beforeDiff = m_BeforeAngle % 360;

            var rotation = m_LightParentTransform.eulerAngles;
            m_Angle = m_BeforeAngle = rotation.z = kMaxRoatationAngle * ratio;

            var afterDiff = m_Angle % 360;

            if (beforeDiff < 30 && afterDiff >= 30)
            {
                if (m_IsFirstBird)
                {
                    PlayBird();
                }
                else
                {
                    if (Random.Range(0, 3) == 0)
                    {
                        PlayBird();
                    }
                }
            }

            PlayBirdRandom(beforeDiff, afterDiff, 5);
            PlayBirdRandom(beforeDiff, afterDiff, 10);
            PlayBirdRandom(beforeDiff, afterDiff, 15);
            PlayBirdRandom(beforeDiff, afterDiff, 20);
            PlayBirdRandom(beforeDiff, afterDiff, 25);

            if (m_Angle >= 90 && m_Angle <= 270)
            {
                if (EnvSoundManager.Instance.Player.LoopName == "env_forest_day")
                {
                    EnvSoundManager.Instance.Player.PlayLoop("env_forest_night", 3.0f);
                }
            }
            else
            {
                if (EnvSoundManager.Instance.Player.LoopName == "env_forest_night")
                {
                    EnvSoundManager.Instance.Player.PlayLoop("env_forest_day", 3.0f);
                }
            }

            m_LightParentTransform.eulerAngles = rotation;

            UpdateIntensity(m_Angle);
            UpdateLightColor(m_Angle);
            UpdateShadowStrength(m_Angle);
            UpdateLampLight();

            if (m_Timer > GameSetting.XdYpLzQaWmNbVoTc)
            {
                m_Timer -= GameSetting.XdYpLzQaWmNbVoTc;
            }
        }

        private void UpdateIntensity(float angle)
        {
            if (angle <= 45f)
            {
                m_Intensity = 2f;
            }
            else if (angle <= 120f)
            {
                // 45〜120度の範囲で、intensityを補完
                m_Intensity = Mathf.Lerp(2f, 0.3f, (angle - 45f) / (120f - 45f));
            }
            else if (angle <= 240f)
            {
                m_Intensity = 0.3f;
            }
            else if (angle <= 315f)
            {
                // 240〜315度の範囲で、intensityを補完
                m_Intensity = Mathf.Lerp(0.3f, 2f, (angle - 240f) / (315f - 240f));
            }
            else
            {
                m_Intensity = 2f;
            }

            m_SunLight.intensity = m_Intensity;
        }

        private void UpdateLightColor(float angle)
        {
            var color = m_SunLight.color;

            // 色変更のロジック
            if (angle <= 30f)
            {
                // 0〜30度ではデフォルト色を維持
                color = m_DefaultColor;
            }
            else if (angle > 30f && angle <= 60f)
            {
                // 30〜60度では Green と Blue を kMinGreen, kMinBlue に近づける
                float t = (angle - 30f) / (60f - 30f); // 30〜60度の間で補完
                byte green = (byte)Mathf.Lerp(m_DefaultColor.g, kMinGreen, t);
                byte blue = (byte)Mathf.Lerp(m_DefaultColor.b, kMinBlue, t);

                color = new Color32(m_DefaultColor.r, green, blue, m_DefaultColor.a);
            }
            else if (angle > 60f && angle <= 240f)
            {
                // 60〜240度ではデフォルト色に近づける
                float t = (angle - 60f) / (240f - 60f); // 60〜240度の間で補完
                byte green = (byte)Mathf.Lerp(kMinGreen, m_DefaultColor.g, t);
                byte blue = (byte)Mathf.Lerp(kMinBlue, m_DefaultColor.b, t);

                color = new Color32(m_DefaultColor.r, green, blue, m_DefaultColor.a);
            }
            else
            {
                // 240〜360度ではデフォルト色を維持
                color = m_DefaultColor;
            }

            m_SunLight.color = color;
        }

        private void UpdateShadowStrength(float angle)
        {
            float shadowStrength = kShadowStrengthMax; // 初期値は最大に設定

            if (angle >= 0f && angle <= 60f)
            {
                // 0〜60度では影の強さは最大 (0.8) を維持
                shadowStrength = kShadowStrengthMax;
            }
            else if (angle > 60f && angle <= 120f)
            {
                // 60〜120度では影の強さが0に近づくように補完
                float t = (angle - 60f) / (120f - 60f); // 0〜1の間で補完
                shadowStrength = Mathf.Lerp(kShadowStrengthMax, kShadowStrengthMin, t);
            }
            else if (angle > 120f && angle <= 240f)
            {
                // 120〜240度では影の強さは最小 (0) を維持
                shadowStrength = kShadowStrengthMin;
            }
            else if (angle > 240f && angle <= 320f)
            {
                // 240〜320度では影の強さが0.8に向かって戻るように補完
                float t = (angle - 240f) / (320f - 240f); // 0〜1の間で補完
                shadowStrength = Mathf.Lerp(kShadowStrengthMin, kShadowStrengthMax, t);
            }
            else if (angle > 320f && angle <= 360f)
            {
                // 320〜360度では影の強さは最大 (0.8) を維持
                shadowStrength = kShadowStrengthMax;
            }

            // 最終的に shadowStrength を m_SunLight に適用
            m_SunLight.shadowStrength = shadowStrength;
        }

        private void UpdateLampLight()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SEPlayer.PlayLampSwitchSE();
                UnityUtility.SetActive(m_LampLightObj, !m_LampLightObj.activeSelf);
            }
        }

        private void PlayBird()
        {
            m_BirdsController.Play();
            m_IsFirstBird = false;
        }

        private void PlayBirdRandom(float beforeDiff, float afterDiff, float diff)
        {
            if (beforeDiff < diff && afterDiff >= diff)
            {
                if (!m_IsFirstBird)
                {
                    if (Random.Range(0, 3) == 0)
                    {
                        PlayBird();
                    }
                }
            }
        }
    }
}
