using System;
using UnityEngine;

namespace curry.Common
{
    public static class AssetPath
    {
        // ====================
        // Manager
        // ====================
        public const string kLocalizationTextDataManagerPrefab = "LocalizeTextDataManager";
        public const string kFaderManagerPrefab = "FaderManager";
        public const string kSaveLoadManagerPrefab = "SaveLoadManager";

        // ====================
        // Prefab
        // ====================
        public const string kTitleUIPrefab = "title_ui";

        // ====================
        // UI
        // ====================

        // ====================
        // Sound
        // ====================
        public const string kSoundFileFormat = "{0}";

        public static string GetSoundFileName(string assetName)
        {
            return string.Format(kSoundFileFormat, assetName);
        }

        public const string kSEAudioMixer = "SeAudioMixer";
    }
}
