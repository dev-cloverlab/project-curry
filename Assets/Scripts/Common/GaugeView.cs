using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Extensions;

namespace curry.Common
{
    public class GaugeView : MonoBehaviour
    {
        private enum GaugeType
        {
            Horizontal, // 横ゲージ
            Vertical, // 縦ゲージ
        }

        [SerializeField, Header("縦ゲージか横ゲージか")]
        private GaugeType m_GaugeType = default;

        [SerializeField, Header("ゲージFillオブジェクト")]
        private RectTransform m_GaugeFill = null;

        [SerializeField, Header("Fillスペース")]
        private RectTransform m_GaugeFillSpace = null;

        [SerializeField, Header("最大ゲージサイズ")]
        private float m_MaxGaugeSize = 0;

        private bool m_IsGaugeMoving;
        /// <summary>
        /// ゲージ増減中かどうか
        /// </summary>
        public bool isGaugeMoving { get { return m_IsGaugeMoving; } }

        private double m_MaxGaugeValue;
        public double maxGaugeValue { get { return m_MaxGaugeValue; } set { m_MaxGaugeValue = value; } }

        private double m_NowGaugeValue;
        public double nowGaugeValue { get { return m_NowGaugeValue; } }

        public bool IsMax => m_NowGaugeValue >= m_MaxGaugeValue;

        private double m_MoveingGaugeValue;
        public double moveingGaugeValue { get { return m_MoveingGaugeValue; } set { m_MoveingGaugeValue = value; } }

        private float m_MaxTime;
        private double m_ReminingGaugeValue;
        private double m_ReminingRate;
        private float m_ReminingTime;

        private bool m_IsPauseMaxOrMin;
        private bool m_IsEnableOver;
        private bool m_IsMinus;
        private bool m_IsPause;
        private bool m_IsStartMax;
        private bool m_IsStartMin;

        public double endValue { get; private set; }

        // ゲージがMaxになった時のコールバック(Maxになった時に何か演出を行ったり、ゲージの最大値変更する場合などに使用)
        public class OnMaxValue : UnityEvent { }

        private OnMaxValue m_OnMaxValue;
        public OnMaxValue onMaxValue { get { return m_OnMaxValue ?? (m_OnMaxValue = new OnMaxValue()); } }

        // ゲージが0になった時のコールバック(↑と同じ)
        public class OnMinValue : UnityEvent { }

        private OnMinValue m_OnMinValue;
        public OnMinValue onMinValue { get { return m_OnMinValue ?? (m_OnMinValue = new OnMinValue()); } }

        // ゲージ増減アニメーション終了時のコールバック
        public class OnAnimationEnd : UnityEvent { }

        private OnAnimationEnd m_OnAnimationEnd;
        public OnAnimationEnd onAnimationEnd { get { return m_OnAnimationEnd ?? (m_OnAnimationEnd = new OnAnimationEnd()); } }

        // ゲージ増減中のイベント
        public class OnAnimationUpdate : UnityEvent { }

        public OnAnimationUpdate onAnimationUpdate { get; } = new OnAnimationUpdate();

        private Sequence m_Sequence;

        private void Awake()
        {
            Init();
        }

        public void Initialize()
        {
            Init();
        }

        private void Init()
        {
            if (m_MaxGaugeSize <= 0)
            {
                switch (m_GaugeType)
                {
                    case GaugeType.Horizontal:
                        m_MaxGaugeSize = m_GaugeFill.sizeDelta.x;
                        break;
                    case GaugeType.Vertical:
                        m_MaxGaugeSize = m_GaugeFill.sizeDelta.y;
                        break;
                }
            }
        }

        public void SetGauge(int now, int max)
        {
            SetGauge((double)now, (double)max);
        }

        public void SetGauge(long now, long max)
        {
            SetGauge((double)now, (double)max);
        }

        public void SetGauge(double now, double max)
        {
            if (max <= 0)
            {
                // LogError("ゲージ最大値に0が設定されました");

                // 最大値0の場合は個別にゲージ0に設定する(0除算回避用)
                // var percent = 0;
                m_NowGaugeValue = 0;
                m_MaxGaugeValue = 0;

                switch (m_GaugeType)
                {
                    case GaugeType.Horizontal:
                    {
                        m_GaugeFill.SetSizeDeltaWidth(0);
                    }
                        break;
                    case GaugeType.Vertical:
                    {
                        m_GaugeFill.SetSizeDeltaHeight(0);
                    }
                        break;
                }

                return;
            }

            m_NowGaugeValue = now;
            m_MaxGaugeValue = max;

            var per = now / max;

            if (per > 1.0)
            {
                per = 1.0;
            }
            else if (per < 0.0)
            {
                per = 0.0;
            }

            switch (m_GaugeType)
            {
                case GaugeType.Horizontal:
                {
                    if (m_MaxGaugeSize <= 0.0)
                    {
                        m_MaxGaugeSize = m_GaugeFill.sizeDelta.x;
                    }

                    m_GaugeFill.SetSizeDeltaWidth((float)(m_MaxGaugeSize * per));
                    if (m_GaugeFillSpace != null)
                    {
                        m_GaugeFillSpace.SetSizeDeltaWidth((float)(m_MaxGaugeSize - (m_MaxGaugeSize * per)));
                    }
                }
                    break;
                case GaugeType.Vertical:
                {
                    if (m_MaxGaugeSize <= 0.0)
                    {
                        m_MaxGaugeSize = m_GaugeFill.sizeDelta.y;
                    }

                    m_GaugeFill.SetSizeDeltaHeight((float)(m_MaxGaugeSize * per));
                    if (m_GaugeFillSpace != null)
                    {
                        m_GaugeFillSpace.SetSizeDeltaHeight((float)(m_MaxGaugeSize - (m_MaxGaugeSize * per)));
                    }
                }
                    break;
            }
        }

        public void SetGaugeRatio(float _ratio)
        {
            var per = Mathf.Clamp(_ratio, 0.0f, 1.0f);

            switch (m_GaugeType)
            {
                case GaugeType.Horizontal:
                {
                    m_GaugeFill.SetSizeDeltaWidth(m_MaxGaugeSize * per);
                }
                    break;
                case GaugeType.Vertical:
                {
                    m_GaugeFill.SetSizeDeltaHeight(m_MaxGaugeSize * per);
                }
                    break;
            }
        }

        /// <summary>
        /// ゲージ増減アニメーション
        /// </summary>
        /// <param name="value">増減する値を入れる 1なら現在の値に+1 -1なら現在の値に-1する</param>
        /// <param name="isEnableOver">trueならMaxになった場合0に戻り、0になった場合Maxに戻る   falseなら最大、最小になった場合にそのままアニメーションを終了する</param>
        /// <param name="isPauseMaxOrMin">trueならゲージが最大or最小になった際に増減を一時停止する、再度アニメーションさせる場合はRestartAnimation()を呼ぶ</param>
        /// <param name="time">ゲージ増減時間</param>
        public void GaugeAnimation(int value, bool isEnableOver, bool isPauseMaxOrMin, float time = 2)
        {
            GaugeAnimation((double)value, isEnableOver, isPauseMaxOrMin, time);
        }

        /// <summary>
        /// ゲージ増減アニメーション
        /// </summary>
        /// <param name="value">増減する値を入れる 1なら現在の値に+1 -1なら現在の値に-1する</param> // TODO 相対値じゃなく絶対値で動かす仕組み欲しい
        /// <param name="isEnableOver">trueならMaxになった場合0に戻り、0になった場合Maxに戻る   falseなら最大、最小になった場合にアニメーションを終了する</param>
        /// <param name="isPauseMaxOrMin">trueならゲージが最大or最小になった際に増減を一時停止する、再度アニメーションさせる場合はRestartAnimation()を呼ぶ</param>
        /// <param name="time">ゲージ増減時間</param>
        public void GaugeAnimation(double value, bool isEnableOver, bool isPauseMaxOrMin, float time = 2)
        {
            if (maxGaugeValue <= 0)
            {
                OnGaugeAnimationEnd();
                return;
            }

            if (value <= 0)
            {
                OnGaugeAnimationEnd();
                return;
            }

            m_IsGaugeMoving = true;

            m_IsMinus = value < 0;

            m_IsStartMax = m_NowGaugeValue >= m_MaxGaugeValue;
            m_IsStartMin = m_NowGaugeValue <= 0;

            m_IsPauseMaxOrMin = isPauseMaxOrMin;
            m_IsEnableOver = isEnableOver;
            m_ReminingGaugeValue = value;

            m_MaxTime = time;

            endValue = 0.0;

            if (m_IsEnableOver)
            {
                endValue = value + m_NowGaugeValue;

                if (!m_IsMinus)
                {
                    m_ReminingGaugeValue = value - (m_MaxGaugeValue - m_NowGaugeValue);
                    m_ReminingRate = (value - (m_MaxGaugeValue - m_NowGaugeValue)) / value;
                }
                else
                {
                    m_ReminingGaugeValue = value + m_NowGaugeValue;

                    // 計算用
                    m_ReminingRate = 1 - ((value - m_ReminingGaugeValue) / value);
                }
            }
            else
            {
                if (!m_IsMinus)
                {
                    if ((m_MaxGaugeValue - m_NowGaugeValue) > value)
                    {
                        endValue = maxGaugeValue;
                    }
                    else
                    {
                        endValue = m_NowGaugeValue + value;
                    }
                }
                else
                {
                    if (m_NowGaugeValue > System.Math.Abs(value))
                    {
                        endValue = 0;
                    }
                    else
                    {
                        endValue = m_NowGaugeValue + value;
                    }

                }
            }

            moveingGaugeValue = m_NowGaugeValue;

            m_Sequence = DOTween.Sequence();
            m_Sequence.Append
                (
                    DOTween.To(() => moveingGaugeValue, (x) => moveingGaugeValue = x, endValue, time)
                )
                .SetEase(Ease.Linear)
                .OnUpdate(OnUpdateAnimation)
                .OnComplete(OnGaugeAnimationEnd)
                ;

            m_Sequence.Play();
        }

        /// <summary>
        /// 一時停止しているアニメーションをリスタートさせる
        /// 一時停止していないタイミングで呼んでも何もしない
        /// リスタート中にMaxValueが変わる場合は直接maxGaugeValueの値を換えてからRestartするかOnMaxValueなどで値を換えてください。
        /// </summary>
        public void RestartAnimation()
        {
            if (!m_IsPause)
            {
                return;
            }

            m_IsPause = false;

            if (moveingGaugeValue >= maxGaugeValue)
            {
                moveingGaugeValue = m_NowGaugeValue = 0;
                SetGauge(moveingGaugeValue, m_MaxGaugeValue);
                GaugeAnimation(m_ReminingGaugeValue, m_IsEnableOver, m_IsPauseMaxOrMin, m_ReminingTime);
            }
            else
            {
                moveingGaugeValue = m_NowGaugeValue = maxGaugeValue;
                SetGauge(moveingGaugeValue, m_MaxGaugeValue);
                GaugeAnimation(m_ReminingGaugeValue, m_IsEnableOver, m_IsPauseMaxOrMin, m_ReminingTime);
            }
        }

        /// <summary>
        /// アニメーションを終了させる。
        /// </summary>
        /// <param name="gaugeValue">終了時の値</param>
        /// <param name="gaugeMax">最大値に変動があれば設定する</param>
        public void GaugeAnimationEnd(float gaugeValue, float gaugeMax = -1)
        {
            m_IsGaugeMoving = false;

            m_Sequence.Kill();
            m_Sequence = null;

            var max = gaugeMax > -1 ? gaugeMax : maxGaugeValue;
            SetGauge(gaugeValue, max);
        }

        private void OnGaugeMaxValue()
        {
            onMaxValue.Invoke();
        }

        private void OnGaugeMinValue()
        {
            onMinValue.Invoke();
        }

        private void OnGaugeAnimationEnd()
        {
            m_IsGaugeMoving = false;
            m_Sequence.Kill();
            m_Sequence = null;
            onAnimationEnd.Invoke();
        }

        private void OnUpdateAnimation()
        {
            SetGauge(moveingGaugeValue, maxGaugeValue);

            if (moveingGaugeValue >= maxGaugeValue || moveingGaugeValue <= 0)
            {
                ResetAnimation(moveingGaugeValue >= maxGaugeValue);
            }

            onAnimationUpdate.Invoke();
        }

        private void ResetAnimation(bool isMax)
        {
            m_ReminingTime = m_MaxTime * (float)m_ReminingRate;
            m_Sequence.OnComplete(() => { }); // KILLしても既にCompleteしている場合は呼ばれてしまうので無効化
            m_Sequence.Kill();
            m_IsPause = true;

            if (isMax)
            {
                if (m_IsStartMax)
                {
                    m_IsStartMax = false;
                    RestartAnimation();
                    return;
                }

                OnGaugeMaxValue();
            }
            else
            {
                if (m_IsStartMin)
                {
                    m_IsStartMin = false;
                    RestartAnimation();
                    return;
                }

                OnGaugeMinValue();
            }

            if (!m_IsEnableOver)
            {
                return;
            }

            if (!m_IsPauseMaxOrMin)
            {
                RestartAnimation();
            }
        }

        [System.Diagnostics.Conditional("CBS_DEBUG")]
        private void LogError(string _text)
        {
            DebugLogWrapper.LogError(string.Format("<color=white>[ゲージ]</color> {0}", _text));
        }
    }
}

