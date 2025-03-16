using UnityEngine;
using UnityEngine.Events;

namespace curry.UI
{
    public class UIBase : MonoBehaviour
    {
        private static readonly int kInitAnimationNameHash = Animator.StringToHash("Init");
        private static readonly int kOpenAnimationNameHash = Animator.StringToHash("Open");
        private static readonly int kIdleAnimationNameHash = Animator.StringToHash("Idle");
        private static readonly int kCloseAnimationNameHash = Animator.StringToHash("Close");
        private static readonly int kClosedAnimationNameHash = Animator.StringToHash("Closed");

        [SerializeField]
        private Animator m_UIAnimator;
        public Animator UIAnimator
        {
            get
            {
                if (m_UIAnimator == null)
                {
                    m_UIAnimator = GetComponent<Animator>();
                }

                return m_UIAnimator;
            }
        }

        [SerializeField]
        private ScrollViewAutoScroll m_ScrollViewAutoScroll;

        private UnityEvent m_OpenedEvent = new UnityEvent();
        public UnityEvent OpenedEvent => m_OpenedEvent;

        private UnityEvent m_ClosedEvent = new UnityEvent();
        public UnityEvent ClosedEvent => m_ClosedEvent;

        public bool IsActivate { get; private set; } = false;

        /// <summary>
        /// パッド入力判定用
        /// </summary>
        /// <param name="active"></param>
        public void Activate(bool active)
        {
            IsActivate = active;
        }

        public void OpenAnimation()
        {
            m_UIAnimator.Play(kOpenAnimationNameHash);
        }

        public void OpenImmediate(bool invokeEvent = true)
        {
            if (invokeEvent)
            {
                OpenedEvent?.Invoke();
            }
            m_UIAnimator.Play(kIdleAnimationNameHash);
        }

        public void CloseAnimation()
        {
            m_UIAnimator.SetTrigger("Close");
        }

        public void CloseImmediate(bool invokeEvent = true)
        {
            if (invokeEvent)
            {
                ClosedEvent?.Invoke();
            }
            m_UIAnimator.Play(kClosedAnimationNameHash);
        }

#region Unity Animation

        public void OnOpened()
        {
            OpenedEvent?.Invoke();
        }

        public void OnClosed()
        {
            m_ClosedEvent?.Invoke();
        }

#endregion

    }
}
