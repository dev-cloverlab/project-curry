using Cysharp.Threading.Tasks;
using UnityEngine;
using curry.Utilities;

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
            // Application.targetFrameRate = Constants.kGameFps;

            InitializeAsync().Forget();
        }

        private static async UniTask InitializeAsync()
        {
            await LocalizationUtility.InitializeAsync();

            IsSetup = true;
        }
    }
}
