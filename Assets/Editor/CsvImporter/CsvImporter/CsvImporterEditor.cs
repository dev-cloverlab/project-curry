using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace curry.CsvImport
{
    public partial class CsvImporterEditor : Editor
    {
        public class StartCell
        {
            public int StartColumnIdx { get; }
            public List<string> KeyNames { get; }
            private readonly string m_MasterName;
            private const string kEOLCode = "\u25b2END";

            public StartCell(StreamReader sr, string masterName)
            {
                m_MasterName = masterName;

                var columnIdx = 0;

                var isExistIdColumn = false;

                var readToEnd = sr.ReadToEnd();

                if (!readToEnd.Contains(kEOLCode))
                {
                    throw new ArgumentException($"\"{kEOLCode}\"が見つかりません {m_MasterName}");
                }

                // 最初に戻す
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                sr.DiscardBufferedData();

                while (!sr.EndOfStream)
                {
                    // 行ごとに見ていく
                    var line = sr.ReadLine();
                    var values = line.Split(',');

                    // IDのセルが無い
                    if (!values.Any(IsIdColumn))
                    {
                        continue;
                    }

                    // IDのセルのカラムのidxを取得
                    columnIdx = Array.FindIndex(values, IsIdColumn);
                    isExistIdColumn = true;

                    // IDリストを取得
                    var tmp = new List<string>( values );
                    KeyNames = tmp.GetRange( columnIdx, values.Length-columnIdx );
                    KeyNames.RemoveAll( s => string.IsNullOrEmpty(s) );

                    break;
                }

                // "id"のセルが見つからなかった場合
                if (!isExistIdColumn)
                {
                    throw new ArgumentException($"\"id\"カラムが見つかりません {m_MasterName}");
                }

                StartColumnIdx = columnIdx;
            }

            private bool IsIdColumn(string text)
            {
                return text is "id";
            }
        }

        private enum ImportSettingType
        {
            None,
            TestRecord,
        }

        private const string kImportPath = "Assets/Csv/MasterData/";
        private const string kExportPath = "Assets/ScriptableObjects/Data/Master/";

        private const string kImportSuffix = ".csv";
        private const string kExportSuffix = ".asset";

        private const string kMenuName = "Data/ImportMasterData/";

        private static void ExportLog(string masterDataName)
        {
            Debug.Log($"<color=green> [MasterDataImport] : {masterDataName} Updated </color>");
        }

        private static string GetImportFilePath(string masterDataName)
        {
            return kImportPath + masterDataName + kImportSuffix;
        }

        private static string GetExportFilePath(string masterDataName)
        {
            return kExportPath + masterDataName + kExportSuffix;
        }

        private static ImportSettingType GetImportSettingType(string[] line)
        {
            const string testRecord = "TestRecord";

            var firstColumn = line[0];

            if (string.IsNullOrEmpty(firstColumn))
            {
                return ImportSettingType.None;
            }

            switch (firstColumn)
            {
                case testRecord:
                    return ImportSettingType.TestRecord;
                default:
                    new ArgumentException("ImportSettingに不正な値が入力されています");
                    break;
            }

            return ImportSettingType.None;
        }

        [MenuItem("Data/ImportAllData", false, 99999)]
        public static void Import()
        {
            // Master
            // Localization
            ImportLocalizationTextData();
        }

        public static int BatchImport()
        {
            try
            {
                Import();
                return 0;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return 1;
            }
        }
    }
}

