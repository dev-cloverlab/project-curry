using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.Audio;
using UnityEngine.U2D;
using System.Collections.Generic;

namespace curry.Common
{
    /// <summary>
    /// シーンをまたいでも常に保持し続けるアセットの読み込み
    /// </summary>
    public class HoldAssetLoadManager : SingletonMonoBehaviour<HoldAssetLoadManager>
    {
        private new void Awake()
        {
            base.Awake();
            base.DontDestroyOnLoad();
        }

        // =================
        // 非同期読み込み
        // =================
        private Dictionary<string, UnityEngine.Object> m_AsyncAddressableAssets = new();
        private Dictionary<string, UnityEngine.Object> m_SyncAddressableAssets = new();

        public async UniTask<Sprite> LoadSpriteAsync(string key, CancellationToken ct)
        {
            return await LoadAsync<Sprite>(key, ct);
        }

        public async UniTask<GameObject> LoadPrefabAsync(string key, CancellationToken ct)
        {
            return await LoadAsync<GameObject>(key, ct);
        }

        public async UniTask<AudioClip> LoadAudioAsync(string key, CancellationToken ct)
        {
            return await LoadAsync<AudioClip>(key, ct);
        }

        public async UniTask<Material> LoadMaterialAsync(string key, CancellationToken ct)
        {
            return await LoadAsync<Material>(key, ct);
        }

        public async UniTask<SpriteAtlas> LoadSpriteAtlasAsync(string key, CancellationToken ct)
        {
            return await LoadAsync<SpriteAtlas>(key, ct);
        }

        private async UniTask<T> LoadAsync<T>(string key, CancellationToken ct) where T : UnityEngine.Object
        {
#if DEBUG_MODE
            var exist = await Exists(key);

            if (!exist)
            {
                DebugLogWrapper.LogAssertion($"Addressable is not found. key : {key} ");
                return null;
            }
#endif

            if (string.IsNullOrEmpty(key))
            {
                DebugLogWrapper.LogError($"Invalid key name {key}");
                return null;
            }

            if (m_AsyncAddressableAssets.ContainsKey(key))
            {
                // 同フレーム呼び出し等でキーだけ設定されている場合はロード完了を待つ
                if (m_AsyncAddressableAssets[key] == null)
                {
                    await UniTask.WaitWhile(() => m_AsyncAddressableAssets[key] == null);
                }

                return m_AsyncAddressableAssets[key] as T;
            }

            m_AsyncAddressableAssets[key] = null;
            var asset = await Addressables.LoadAssetAsync<T>(key);
            m_AsyncAddressableAssets[key] = asset;

            return asset;
        }

        public async void LoadAssetsFromLabel<T>(string label) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            await handle.Task;
#if DEBUG_MODE
            // ロードしたアセット一覧
            // 一応確認用にコメントで残しておく
            // foreach (var result in handle.Result)
            // {
            //     DebugLogWrapper.Log($"<color=white> [[Debug]] : SE Load [{result}] </color>");
            // }
#endif
        }

        // =================
        // 同期読み込み
        // =================

        public Sprite LoadSprite(string key)
        {
            return Load<Sprite>(key);
        }

        public GameObject LoadPrefab(string key)
        {
            return Load<GameObject>(key);
        }

        public Material LoadMaterial(string key)
        {
            return Load<Material>(key);
        }

        public AudioMixer LoadAudioMixer(string key)
        {
            return Load<AudioMixer>(key);
        }

        public T Load<T>(string key) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key))
            {
                DebugLogWrapper.LogError($"Invalid key name {key}");
                return null;
            }

            if (m_SyncAddressableAssets.TryGetValue(key, out var loadedAsset))
            {
                return loadedAsset as T;
            }

            try
            {
                var asset = Addressables.LoadAssetAsync<T>(key).WaitForCompletion();
                m_SyncAddressableAssets[key] = asset;
                return asset;
            }
            catch (Exception e)
            {
                DebugLogWrapper.LogAssertion($"{e}");
                return null;
            }
        }

        public static async UniTask<bool> Exists(string pathToAsset)
            => (await Addressables.LoadResourceLocationsAsync(pathToAsset)).Any();
    }
}

