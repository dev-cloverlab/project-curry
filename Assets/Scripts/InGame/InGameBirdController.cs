using curry.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Extensions;
using UnityEngine;


namespace curry.InGame
{
    public class InGameBirdController : MonoBehaviour
    {
        [SerializeField]
        private Animator m_Animator;

        [SerializeField]
        private Transform m_BirdTransform;

        [SerializeField]
        private GameObject m_BirdGameObject;

        [SerializeField]
        private float m_AnimationSpeedBase;

        private static readonly int m_BirdAnimationHash = Animator.StringToHash("Play");

        private float m_MoveSpeed;
        private float m_NextAnimationTime;

        private const float kStartZ = -13.69f;
        private const float kEndZ = 5.0f;
        private const float kMoveSpeedMin = 0.03f;
        private const float kMoveSpeedMax = 0.07f;
        private const float kNextAnimationMin = 0.85f;
        private const float kNextAnimationMax = 1.0f;

        public void Play()
        {
            UnityUtility.SetActive(m_BirdGameObject, true);

            var speed = Random.Range(m_AnimationSpeedBase - (m_AnimationSpeedBase / 10), m_AnimationSpeedBase + (m_AnimationSpeedBase / 10));
            m_Animator.speed = speed;
            m_Animator.PlayImmediate(m_BirdAnimationHash);

            var pos = m_BirdTransform.localPosition;
            pos.z = kStartZ;
            m_BirdTransform.localPosition = pos;

            SetNextAnimationTime();

            m_MoveSpeed = Random.Range(kMoveSpeedMin, kMoveSpeedMax);

            Move().Forget();
        }

        private async UniTask Move()
        {
            while (true)
            {
                var pos = m_BirdTransform.localPosition;
                pos.z += m_MoveSpeed;
                m_BirdTransform.localPosition = pos;

                if (pos.z >= kEndZ)
                {
                    break;
                }

                await UniTask.Yield();

                var currentAnimatorState = m_Animator.GetCurrentAnimatorStateInfo(0);

                if (currentAnimatorState.normalizedTime >= m_NextAnimationTime)
                {
                    SetNextAnimationTime();
                    m_Animator.PlayImmediate(m_BirdAnimationHash);
                }
            }
        }

        private void SetNextAnimationTime()
        {
            m_NextAnimationTime = Random.Range(kNextAnimationMin, kNextAnimationMax);
        }
    }
}
