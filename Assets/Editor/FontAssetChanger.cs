using UnityEngine;
using UnityEditor;
using TMPro;

public class FontAssetChanger : EditorWindow
{
    [MenuItem("Tools/Change TMP Font Asset in Prefabs")]
    public static void ChangeFontAssetInPrefabs()
    {
        string fontAssetName = "MPLUS1-Bold SDF";
        TMP_FontAsset targetFontAsset = null;

        // フォントアセットを先に取得
        string[] fontGuids = AssetDatabase.FindAssets(fontAssetName + " t:TMP_FontAsset");
        if (fontGuids.Length > 0)
        {
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]); // [0]が抜けてた
            targetFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        }
        else
        {
            Debug.LogError($"Font Asset '{fontAssetName}' not found in project.");
            return;
        }

        // Assets以下のすべてのプレハブを取得
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets"});
        int changedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool prefabChanged = false;

            // TextMeshProとTextMeshProUGUIの両方を処理
            TextMeshPro[] tmpros = prefab.GetComponentsInChildren<TextMeshPro>(true);
            TextMeshProUGUI[] tmproUGUIs = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            // TextMeshProのFont Assetを変更
            foreach (var tmp in tmpros)
            {
                if (tmp.font != targetFontAsset)
                {
                    Undo.RecordObject(tmp, "Change TMP Font Asset");
                    tmp.font = targetFontAsset;
                    EditorUtility.SetDirty(tmp);
                    prefabChanged = true;
                }
            }

            // TextMeshProUGUIのFont Assetを変更
            foreach (var tmpUGUI in tmproUGUIs)
            {
                if (tmpUGUI.font != targetFontAsset)
                {
                    Undo.RecordObject(tmpUGUI, "Change TMP Font Asset");
                    tmpUGUI.font = targetFontAsset;
                    EditorUtility.SetDirty(tmpUGUI);
                    prefabChanged = true;
                }
            }

            if (prefabChanged)
            {
                changedCount++;
                Debug.Log($"Changed font in prefab: {path}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Font Asset change completed. {changedCount} prefabs were modified.");
    }
}
