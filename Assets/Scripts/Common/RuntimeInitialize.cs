using curry.Localization;
using curry.Ranking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using curry.Utilities;
using curry.Sound;
using Steamworks;

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

            BgmManager.Create();
            EnvSoundManager.Create();
            FireSoundManager.Create();
            BgmManager.Instance.Player.VolumeSet(20);
            EnvSoundManager.Instance.Player.VolumeSet(20);
            FireSoundManager.Instance.Player.VolumeSet(20);

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
            CurryLeaderBoardManager.Create();
            // SteamManager初期化
            await CurryLeaderBoardManager.Instance.Init();

            // 指定の言語に合わせる
            LocalizationTextManager.ChangeText();
            IsSetup = true;
        }
    }
}
