using UnityEngine;

namespace curry.Common
{
    public class UICameraSettings : MonoBehaviour
    {
        [SerializeField]
        private Camera m_UICamera;

        private void Awake()
        {
            FaderManager.Instance.FaderCanvas.worldCamera = m_UICamera;
        }
    }
}

    