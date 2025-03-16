using UnityEngine;

namespace curry.InGame
{
    public class CarrotController : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody m_Rigidbody;

        private Transform m_CarrotTransform;

        private const float kRadius = 1.3f;
        private float m_RadiusSqr;

        private bool m_IsUseGravity;

        private void Awake()
        {
            m_CarrotTransform = transform;
            m_RadiusSqr = kRadius * kRadius;
        }

        private void LateUpdate()
        {
            if (m_IsUseGravity)
            {
                return;
            }

            var x = m_CarrotTransform.position.x;
            var z = m_CarrotTransform.position.z;

            // 半径をの二乗計算
            var distanceSqr = x * x + z * z;

            // 半径が制限を超えている場合のみ修正
            if (distanceSqr > m_RadiusSqr)
            {
                m_IsUseGravity = true;
                m_Rigidbody.useGravity = true;
                m_Rigidbody.constraints = RigidbodyConstraints.None;
            }
        }
    }
}
