using System;
using UnityEngine;

namespace curry.InGame
{
    public class LadleAngleController : MonoBehaviour
    {
        [SerializeField]
        private Transform m_LadleRootTransform;

        [SerializeField]
        private Transform m_LadleEntityTransform;
        private Vector3 m_LadleEntityEulerAngles;

        private void Awake()
        {
            m_LadleEntityEulerAngles = m_LadleEntityTransform.eulerAngles;
        }

        public void LadleRotate(float magnitude)
        {
            magnitude = Mathf.Min(magnitude, GameSetting.ThSrhgVLuxXcPTSH);
            var eulerAngles = m_LadleRootTransform.eulerAngles;
            eulerAngles.y += magnitude * GameSetting.qgUjmtKLExaPGUTJ;
            m_LadleRootTransform.eulerAngles = eulerAngles;
        }

        private void LateUpdate()
        {
            m_LadleEntityTransform.eulerAngles = m_LadleEntityEulerAngles;
        }
    }
}
