using Cysharp.Threading.Tasks;
using UnityEngine;
using curry.Utilities;
using curry.Sound;

namespace curry.Common
{
    public static class RuntimeInitialize
    {
        public static bool IsSetup { get; private set; }

        /// <summary>
        /// 何より先に呼び出される
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            QualitySettings.vSyncCount = -1;
            Application.targetFrameRate = Constants.kGameFps;

            AssetLoadManager.Create();
            HoldAssetLoadManager.Create();

            FaderManager.Create();
            LocalizeTextDataManager.Create();
            LocalizeTextDataManager.Instance.Setup();

            SEPlayer.Create();
            SEPlayer.VolumeSet(10);
            SEPlayer.HoldLoadAllSE();

            BgmPlayer.Init();
            BgmPlayer.VolumeSet(10);

            UserDataManager.Create();
            SaveLoadManager.Create();

#if DEBUG_MODE

            DebugLogWrapper.Log("<color=white>  DEBUG_MODE  </color>");

#endif
            InitializeAsync().Forget();

            FaderManager.Instance.FadeOutImmediate();
        }

        private static async UniTask InitializeAsync()
        {
            await SaveLoadManager.Instance.InitializeAsync();
            await LocalizationUtility.InitializeAsync();
#if DEBUG_MODE

            var locale = LocalizationUtility.GetCurrentLocale();
            DebugLogWrapper.Log($"<color=white> Locale {locale} </color>");

#endif
            IsSetup = true;
        }
    }
}
