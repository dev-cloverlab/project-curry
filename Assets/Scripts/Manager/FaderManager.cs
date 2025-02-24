using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace curry.Common
{
    public class FaderManager : SingletonMonoBehaviour<FaderManager>
    {
        [SerializeField] private CanvasGroup m_BlackPlateCanvasGroup;
        [SerializeField] private Canvas m_FaderCanvas;
        public Canvas FaderCanvas => m_FaderCanvas;

        private const float kFadeOutAlpha = 1.0f;
        private const float kFadeInAlpha = 0.0f;
        private const float kDefaultFadeTime = 1.0f;
        private const float kShortFadeTime = 0.1f;

        public new static void Create()
        {
            if (IsInstance())
            {
                return;
            }

            var prefab = HoldAssetLoadManager.Instance.LoadPrefab(AssetPath.kFaderManagerPrefab);
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = "FaderManager";
        }

        private new void Awake()
        {
            base.Awake();

            if (!IsThisInstance())
            {
                return;
            }

            DontDestroyOnLoad();
            PreLoadingDisable();
        }

        private void PreLoadingDisable()
        {
            FadeInImmediate();
        }

        public bool IsFadeOutEnd()
        {
            return m_BlackPlateCanvasGroup.alpha >= kFadeOutAlpha;
        }

        public bool IsFadeInEnd()
        {
            return m_BlackPlateCanvasGroup.alpha <= kFadeInAlpha;
        }

        /// <summary>
        /// 指定秒数で画面を暗転させる
        /// </summary>
        public async UniTask FadeOut(float fadeTime = kDefaultFadeTime)
        {
            if (m_BlackPlateCanvasGroup.alpha <= kFadeInAlpha)
            {
                await m_BlackPlateCanvasGroup.DOFade(kFadeOutAlpha, fadeTime).AsyncWaitForCompletion();
            }
        }

        /// <summary>
        /// 指定秒数で暗転を開ける
        /// </summary>
        public async UniTask FadeIn(float fadeTime = kDefaultFadeTime)
        {
            if (m_BlackPlateCanvasGroup.alpha >= kFadeOutAlpha)
            {
                await m_BlackPlateCanvasGroup.DOFade(kFadeInAlpha, fadeTime).AsyncWaitForCompletion();
            }
        }

        public async UniTask FadeOutShort()
        {
            await FadeOut(kShortFadeTime);
        }

        public async UniTask FadeInShort()
        {
            await FadeIn(kShortFadeTime);
        }

        /// <summary>
        /// すぐに画面を暗転させる
        /// </summary>
        public void FadeOutImmediate()
        {
            SetFadeAlpha(kFadeOutAlpha);
        }

        /// <summary>
        /// すぐに暗転を開ける
        /// </summary>
        public void FadeInImmediate()
        {
            SetFadeAlpha(kFadeInAlpha);
        }

        private void SetFadeAlpha(float alpha)
        {
            m_BlackPlateCanvasGroup.alpha = alpha;
        }
    }
}

    