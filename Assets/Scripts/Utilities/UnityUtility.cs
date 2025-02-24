using UnityEngine;

namespace curry.Utilities
{
    public static class UnityUtility
    {
        private const string PrefabTag = " (Clone)";

        /// <summary> Prefabからインスタンス生成 </summary>
        public static T Instantiate<T>(UnityEngine.Object prefab, Transform parent,
            bool instantiateInWorldSpace = false) where T : Component
        {
            var gameObject = Instantiate(prefab, parent, instantiateInWorldSpace);

            return GetComponent<T>(gameObject);
        }

        /// <summary> Prefabからインスタンス生成 </summary>
        public static GameObject Instantiate(UnityEngine.Object prefab, Transform parent,
            bool instantiateInWorldSpace = false)
        {
            if (prefab != null)
            {
                GameObject gameObject = null;

                if (parent == null)
                {
                    gameObject = UnityEngine.Object.Instantiate(prefab) as GameObject;
                }
                else
                {
                    gameObject = UnityEngine.Object.Instantiate(prefab, parent, instantiateInWorldSpace) as GameObject;
                }

                if (gameObject != null)
                {
                    gameObject.transform.name = prefab.name + PrefabTag;
                }

                return gameObject;
            }

            return null;
        }

        /// <summary> コンポーネント取得 </summary>
        private static T GetComponent<T>(GameObject instance) where T : Component
        {
            return instance != null ? instance.GetComponent<T>() : null;
        }

        /// <summary> 状態設定 </summary>
        public static void SetActive(GameObject instance, bool state)
        {
            if (instance == null) { return; }

            if (state == instance.activeSelf) { return; }

            instance.SetActive(state);
        }

        /// <summary> 状態設定 </summary>
        public static void SetActive<T>(T instance, bool state) where T : Component
        {
            if (instance == null) { return; }

            if (state == instance.gameObject.activeSelf) { return; }

            instance.gameObject.SetActive(state);
        }

#if DEBUG_MODE

        /// <summary>
        /// ヒエラルキー上のフルパスを取得
        /// デバッグ機能用
        /// </summary>
        public static string GetHierarchyFullPath(this GameObject obj)
        {
            if (obj == null)
            {
                DebugLogWrapper.LogError($"<color=white> [[Debug]] : GameObject is null </color>");
                return null;
            }

            return GetHierarchyFullPath(obj.transform);
        }

        /// <summary>
        /// ヒエラルキー上のフルパスを取得
        /// デバッグ機能用
        /// </summary>
        public static string GetHierarchyFullPath(this Transform t)
        {
            if (t == null)
            {
                DebugLogWrapper.LogError($"<color=white> [[Debug]] : Transform is null </color>");
                return null;
            }

            string path = t.name;
            var parent = t.parent;
            while (parent)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }

#endif
    }
}
