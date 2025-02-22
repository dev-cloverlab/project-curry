using System;
using UnityEngine;

namespace curry.Common
{
    public static class AssetPath
    {
        // ====================
        // Manager
        // ====================
        public const string kMasterDataManagerPrefab = "MasterDataManager.prefab";
        public const string kLocalizationTextDataManagerPrefab = "LocalizationTextDataManager.prefab";

        // ====================
        // Prefab
        // ====================

        // ====================
        // UI
        // ====================

        // ====================
        // Sound
        // ====================
        public const string kSoundFileFormat = "{0}.ogg";

        public static string GetSoundFileName(string assetName)
        {
            return string.Format(kSoundFileFormat, assetName);
        }

        public const string kSEAudioMixer = "SeAudioMixer.mixer";
    }
}
