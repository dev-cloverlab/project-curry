using curry.Utilities;
using TMPro;
using UnityEngine;

namespace curry.Localization
{
    /// <summary>
    /// TextMeshProに書かれたテキストを指定のキーのテキストでローカライズ
    /// </summary>
    public class LocalizationText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_TargetTextGUI;

        [SerializeField]
        private TextMeshPro m_TargetText;

        [SerializeField]
        private string m_Key;

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_TargetTextGUI = this.GetComponent<TextMeshProUGUI>();
            m_TargetText = this.GetComponent<TextMeshPro>();
        }
#endif

        private void Awake()
        {
#if DEBUG_MODE
            if (string.IsNullOrEmpty(m_Key))
            {
                DebugLogWrapper.LogAssertion($"Localization key is Empty. Pass:{transform.GetHierarchyFullPath()}");
                return;
            }
#endif

#if DEBUG_MODE
            if (m_TargetTextGUI == null && m_TargetText != null)
            {
                DebugLogWrapper.LogAssertion($"TextMeshPro is null. Pass:{transform.GetHierarchyFullPath()}");
                return;
            }
#endif

            if (m_TargetTextGUI != null)
            {
                m_TargetTextGUI.SetText(LocalizationUtility.GetLocalizeText(m_Key));
            }

            if (m_TargetText != null)
            {
                m_TargetText.SetText(LocalizationUtility.GetLocalizeText(m_Key));
            }
        }
    }
}

    