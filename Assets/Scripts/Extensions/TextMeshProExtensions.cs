using curry.Utilities;
using TMPro;
using UnityEngine;

namespace Extensions
{
    public static class TextMeshProExtensions
    {
        public static void SetLocalizeText(this TextMeshProUGUI textMeshProUGUI, string key)
        {
            textMeshProUGUI.SetText(LocalizationUtility.GetLocalizeText(key));
        }

        public static void SetLocalizeText(this TextMeshPro textMeshPro, string key)
        {
            textMeshPro.SetText(LocalizationUtility.GetLocalizeText(key));
        }
    }
}
