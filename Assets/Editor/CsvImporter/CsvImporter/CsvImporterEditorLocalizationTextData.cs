using System;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using curry.Scriptable;
using curry.Text;
using UnityEngine;

namespace curry.CsvImport
{
    public partial class CsvImporterEditor
    {
        private const string kLocalizationTextDataName = "LocalizeTextData";

        private const string kLocalizationImportPath = "Assets/Csv/Localization/";
        private const string kLocalizationExportPath = "Assets/ScriptableObjects/Data/LocalizeText/";

        private const string kLocalizationMenuName = "Data/ImportLocalizeData/";

        private static string GetLocalizationTextImportFilePath(string dataName)
        {
            return kLocalizationImportPath + dataName + kImportSuffix;
        }

        private static string GetLocalizationTextExportFilePath(string dataName)
        {
            return kLocalizationExportPath + dataName + kExportSuffix;
        }

        [MenuItem(kLocalizationMenuName + kLocalizationTextDataName, false, 22)]
        private static void ImportLocalizationTextData()
        {
            var dataName = kLocalizationTextDataName;

            var importFile = GetLocalizationTextImportFilePath(dataName);
            var exportFile = GetLocalizationTextExportFilePath(dataName);

            // 既存のマスタを取得
            var so = AssetDatabase.LoadAssetAtPath<LocalizationTextDataScriptableObject>(exportFile);

            // 見つからなければ作成する
            if (so == null)
            {
                so = CreateInstance<LocalizationTextDataScriptableObject>();
                AssetDatabase.CreateAsset(so, exportFile);
            }

            var dataList = new List<LocalizationTextData>();

            List<string> keyList = new ();

            // CSVファイルをオブジェクトへ保存
            using( FileStream fs = new FileStream( importFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    // ヘッダをやり過ごす
                    sr.ReadLine();
                    sr.ReadLine();

                    // ファイルの終端まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        string[] values = line.Split(',');

                        // 区切りをスキップ
                        if (values.Length <= 0 || string.IsNullOrEmpty(values[0]))
                        {
                            continue;
                        }

                        // 値を設定する
                        var idx = 0;

                        // 同じキーが複数あったらエラーを吐く
                        if (keyList.Contains(values[idx]))
                        {
                            Debug.LogError($"<color=red> [LocalizationTextData] ERROR. Entry with the same key already exists [{values[idx]}]</color>");
                            continue;
                        }

                        // 追加するパラメータを生成
                        var data = new LocalizationTextData();

                        keyList.Add(values[idx]);
                        data.Key = values[idx];

                        idx++;
                        data.Ja = values[idx];

                        idx++;
                        data.En = values[idx];

                        dataList.Add(data);
                    }
                }
            }

            so.DataList = dataList;

            // 保存
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(so);

            ExportLog(dataName);
        }
    }
}