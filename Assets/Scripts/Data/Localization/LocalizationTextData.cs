using UnityEngine;
using curry.Enums;

namespace curry.Text
{
    public interface ILocalizationTextData
    {
        public string Key { get; set; }
        public string Ja { get; set; }
        public string En { get; set; }

        public string GetLocalizationText(LocalizationLocaleType localeType);
    }

    [System.Serializable]
    public class LocalizationTextData : ILocalizationTextData
    {
        [SerializeField]
        private string key;
        public string Key { get { return key; } set { key = value; } }

        [SerializeField]
        private string ja;
        public string Ja { get { return ja; } set { ja = value; } }

        [SerializeField]
        private string en;
        public string En { get { return en; } set { en = value; } }

        public string GetLocalizationText(LocalizationLocaleType locale)
        {
            switch (locale)
            {
                default:
                    DebugLogWrapper.LogAssertion($"invalid locale type {locale}");
                    return en;
                case LocalizationLocaleType.en:
                    return en;
                case LocalizationLocaleType.ja:
                    return ja;
            }
        }
    }
}
