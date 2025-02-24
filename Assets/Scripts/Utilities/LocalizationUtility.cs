using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Application = UnityEngine.Device.Application;
using curry.Text;
using curry.Enums;
using curry.Common;
using System.Linq;
#if UNITY_EDITOR
using curry.Scriptable;
using UnityEditor;
#endif

namespace curry.Utilities
{
    public static class LocalizationUtility
    {
#if UNITY_SWITCH
        private static readonly Dictionary<string, LocalizationLocaleType> kLocalizationLanguageMap = new Dictionary<string, LocalizationLocaleType>(){
            { "en-US", LocalizationLocaleType.en },
            { "ja", LocalizationLocaleType.ja },
            // { "en-GB", "British English" },
            // { "fr", "French" },
            // { "de", "German" },
            // { "es-419", "LatinAmerican Spanish" },
            // { "es", "Spanish" },
            // { "it", "Italian" },
            // { "nl", "Dutch" },
            // { "fr-CA", "Canadian French" },
            // { "pt", "Portuguese" },
            // { "ru", "Russian" },
            // { "zh-Hans", "Simplified Chinese" },
            // { "zh-Hant", "Traditional Chinese" },
            // { "ko", "Korean" },
        };
#endif

        /// <summary>
        /// デフォルトの言語を取得
        /// 対応外の言語の場合は英語を返します
        /// </summary>
        /// <returns></returns>
        public static LocalizationLocaleType GetDefalutLocaleType()
        {
            LocalizationLocaleType localeType = LocalizationLocaleType.none;

#if UNITY_SWITCH && !UNITY_EDITOR
// Switch向け
            DebugLogWrapper.Log($"<color=white> [[Switch]] : Desired {nn.oe.Language.GetDesired()} </color>");

            // SystemLanguageを取得して言語を設定
            var desired = nn.oe.Language.GetDesired();

            if (kLocalizationLanguageMap.TryGetValue(desired, out var tempLocaleType))
            {
                localeType = tempLocaleType;
            }
            else
            {
                DebugLogWrapper.LogAssertion($"invalid key {desired}");
                localeType = LocalizationLocaleType.en;
            }
#else
// PC向け

            DebugLogWrapper.Log($"<color=white> [[Debug]] : Application.systemLanguage {Application.systemLanguage} </color>");

            switch (Application.systemLanguage)
            {
                default:
                case SystemLanguage.English:
                    DebugLogWrapper.Log($"<color=white> [[Debug]] : System Language English </color>");
                    localeType = LocalizationLocaleType.en;
                    break;
                case SystemLanguage.Japanese:
                    DebugLogWrapper.Log($"<color=white> [[Debug]] : System Language Japanese </color>");
                    localeType = LocalizationLocaleType.ja;
                    break;
            }
#endif
            return localeType;
        }


        public static async UniTask InitializeAsync()
        {
            // Reset initialization state. (Unload all tables)
            LocalizationSettings.Instance.ResetState();

            LocalizationLocaleType localeType = UserDataManager.Instance.Option.m_Language;

            await SetLocale(localeType);
        }

        public static async UniTask SetLocale(LocalizationLocaleType locale)
        {
            // Set locale with locale code.
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales.Find(x => x.Identifier.Code == GetLocaleString(locale));

            // Wait initialization.
            await LocalizationSettings.InitializationOperation.Task;
        }

        public static LocalizationLocaleType GetCurrentLocale()
        {
            var locale = LocalizationSettings.SelectedLocale;

            var localizationType = GetLocale(locale.LocaleName);
            return localizationType;
        }

        public static LocalizationLocaleType GetLocale(string locale)
        {
            switch (locale)
            {
                default:
                    DebugLogWrapper.LogAssertion($"invalid locale name {locale}");
                    return LocalizationLocaleType.en;
                case "en":
                    return LocalizationLocaleType.en;
                case "ja":
                    return LocalizationLocaleType.ja;
            }
        }

        public static string GetLocaleString(LocalizationLocaleType locale)
        {
            switch (locale)
            {
                case LocalizationLocaleType.none:
                    DebugLogWrapper.LogAssertion($"invalid locale type {locale}");
                    return "en";
                default:
                case LocalizationLocaleType.en:
                    return "en";
                case LocalizationLocaleType.ja:
                    return "ja";
            }
        }

        public static string GetLocalizeText(string key)
        {
            return LocalizeTextDataManager.Instance.Get<LocalizationTextData>(key, GetCurrentLocale());
        }

#if UNITY_EDITOR
        private const string kLocalizationTextDataName = "LocalizationTextData";
        private const string kLocalizationExportPath = "Assets/ScriptableObjects/Data/LocalizationText/";
        private const string kExportSuffix = ".asset";

        public static string GetLocalizationTextExportFilePath()
        {
            return kLocalizationExportPath + kLocalizationTextDataName + kExportSuffix;
        }

        public static string GetLocalizeTextEditor(string key)
        {
            var exportFile = GetLocalizationTextExportFilePath();

            // 既存のマスタを取得
            var so = AssetDatabase.LoadAssetAtPath<LocalizationTextDataScriptableObject>(exportFile);
            var data = so.DataList.FirstOrDefault(x => x.Key == key);

            if (data == null)
            {
                Debug.LogAssertion($"ローカライズキーに{key}が設定されていません");
                return string.Empty;
            }

            return data.Ja;
        }
#endif
    }
}
