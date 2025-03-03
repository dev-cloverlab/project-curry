using System;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Compilation;
using UnityEngine;

namespace curry.CsvImport
{
    public partial class CsvImporterEditor
    {
        private const string kGameSettingDataName = "GameSettingData";
        private const string kGameSettingExportPath = "Assets/Scripts/Data/GameSetting";

        [MenuItem(kMenuName + "Remove" + kGameSettingDataName, false, 11)]
        private static void RemoveGameSetting()
        {
            var exportFile = kGameSettingExportPath + "/_" + kGameSettingDataName + ".cs";
            File.Delete(exportFile);
        }

        [MenuItem(kMenuName + kGameSettingDataName, false, 11)]
        private static void ImportGameSettingData()
        {
            var masterName = kGameSettingDataName;

            var importFile = GetImportFilePath(masterName);
            var exportFile = kGameSettingExportPath + "/_" + kGameSettingDataName + ".cs";

            // 一旦ファイル削除
            File.Delete(exportFile);
            var nowDt = DateTime.Now;
            var variableTabLength = 24;

            List<string> logList = new();

            // CSVファイルをオブジェクトへ保存
            using( FileStream fs = new FileStream( importFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite ) )
            {
                using (StreamReader sr = new StreamReader(fs))
                using (StreamWriter sw = new StreamWriter(exportFile, true, Encoding.UTF8))
                {

                    sw.WriteLine("/*");
                    sw.WriteLine($" * 出力日 {nowDt:yyyy/MM/dd} ({nowDt:ddd}) {nowDt:HH:mm:ss}");
                    sw.WriteLine(" * ");
                    sw.WriteLine(" * CsvGameSettingImporterEditor.cs から自動出力されるファイルです");
                    sw.WriteLine(" * このファイルは直接編集しないでください");
                    sw.WriteLine(" */");

                    sw.WriteLine("// ReSharper disable All");
                    sw.WriteLine("public static class GameSetting");
                    sw.WriteLine("{");

                    // 1行目を飛ばす
                    sr.ReadLine();

                    // ファイルの終端まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] values = line.Split(',');

                        var idx = 0;

                        if (string.IsNullOrEmpty(values[idx]))
                        {
                            continue;
                        }

                        {
                            var outputDescription = values[idx++];
                            var outputLine = $"    /// <summary> {outputDescription} </summary>";
                            sw.WriteLine(outputLine);
                        }

                        {
                            var outputLine = "    public static readonly ";

                            var outputType = values[idx++];

                            var typeLength = outputType.Length;
                            var diffLength = variableTabLength - typeLength;

                            var outputVariable = values[idx++];
                            var outputValue = values[idx++];

                            var denom = string.Empty;
                            switch (outputType)
                            {
                                case "float":
                                    denom = "f";
                                    break;
                                case "decimal":
                                    denom = "m";
                                    break;
                            }

                            outputLine += $"Protector<{outputType}>{outputVariable.PadLeft(diffLength, ' ')} = {outputValue}{denom};";
                            logList.Add(outputLine);
                            sw.WriteLine(outputLine);
                        }
                    }

                    sw.WriteLine("}");
                }
            }

            // 保存
            ExportLog(masterName);

            // 強制コンパイル
            CompilationPipeline.RequestScriptCompilation();

            foreach (var log in logList)
            {
                DebugLogWrapper.Log($"<color=cyan> [[GameSetting]] : {log} </color>");
            }
        }
    }
}