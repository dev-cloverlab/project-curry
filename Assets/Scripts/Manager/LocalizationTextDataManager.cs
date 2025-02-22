using curry.Scriptable;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using curry.Text;
using UnityEngine.Serialization;
using curry.Enums;
using curry.Common;

namespace curry.Common
{
    public class LocalizationTextDataManager : SingletonMonoBehaviour<LocalizationTextDataManager>
    {
        [FormerlySerializedAs("m_MasterGameSettings")]
        [SerializeField]
        private LocalizationTextDataScriptableObject m_LocalizationTextData;

        public new static void Create()
        {
            if (IsInstance())
            {
                return;
            }

            var prefab = HoldAssetLoadManager.Instance.LoadPrefab(AssetPath.kLocalizationTextDataManagerPrefab);
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = "LocalizationTextDataManager";
        }

        private new void Awake()
        {
            base.Awake();

            if (!IsThisInstance())
            {
                return;
            }

            DontDestroyOnLoad();
        }

        public void Setup()
        {
            SetPool<LocalizationTextData>(m_LocalizationTextData.DataList);
        }

        /// <summary>
        /// 1件取得する
        /// 存在しなければアサートを吐いて空文字を返す
        /// </summary>
        public string Get<T>(string key, LocalizationLocaleType localeType) where T : ILocalizationTextData
        {
            var dic = GetPool<T>();

            if (dic.TryGetValue(key, out var result))
            {
                return result.GetLocalizationText(localeType);
            }

            DebugLogWrapper.LogAssertion($"localization text not found key: {key}");

            return string.Empty;
        }

        /// <summary>
        /// プールに値をセットする
        /// 値は上書きされる
        /// </summary>
        private static void SetPool<T>(IEnumerable<T> dataList) where T : ILocalizationTextData
        {
            DataCache<string, T>.cache = dataList.ToDictionary(x => x.Key, x => x);
        }

        /// <summary>
        /// プールから取り出す
        /// </summary>
        /// <typeparam name="T">プールされたデータ</typeparam>
        /// <returns></returns>
        private static Dictionary<string, T> GetPool<T>()
        {
            return DataCache<string, T>.cache;
        }

        /// <summary>
        /// DataCacheインスタンスリスト
        /// </summary>
        private static List<IDictionary> cacheList = new ();

        private class DataCache<Key, Value>
        {
            public static Dictionary<Key, Value> cache;

            static DataCache()
            {
                cache = new Dictionary<Key, Value>();
                lock (cacheList)
                {
                    cacheList.Add(cache);
                }
            }
        }
    }
}
