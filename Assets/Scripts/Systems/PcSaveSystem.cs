#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace curry.Common
{
    public class PcSaveSystem : ISaveSystem
    {
#if STEAM   // TODO 定義名忘れたので後で差し替える
        private static readonly string kRootFolder = Application.persistentDataPath + "/save";
#else
        private static readonly string kRootFolder = Application.dataPath + "/../Save";
#endif
#if USE_JSONDATA && UNITY_STANDALONE
        private static readonly string kFileName = "playdata.json";
#else
        private static readonly string kFileName = "playdata.save";
#endif

        /// <summary>
        /// ルートディレクトリ
        /// SteamとPCで分岐
        /// </summary>
        private static string RootDirectory
        {
            get {
#if STEAM   // TODO 定義名忘れたので後で差し替える
                string steamID = Steamworks.SteamUser.GetSteamID().ToString();
                return Path.Combine( kRootFolder, steamID );
#else
                return kRootFolder;
            }
#endif
        }

        /// <summary>
        /// ファイルパス
        /// 現状は1ファイルのみ想定のため固定
        /// </summary>
        private static string FilePath
        {
            get { return Path.Combine( RootDirectory, kFileName ); }
        }
        
        public PcSaveSystem()
        {
            if( !Directory.Exists( RootDirectory ) ) {
                Debug.Log( $"[PcSaveSystem]フォルダがないため作成します⇒{RootDirectory}" );
                Directory.CreateDirectory( RootDirectory );
            }
        }

        /// <summary>
        /// ファイルが存在しているか
        /// </summary>
        /// <returns></returns>
        public override bool IsExistFile()
        {
            return File.Exists( FilePath );
        }

        /// <summary>
        /// 実際の書き込み処理実体
        /// Thread内で呼び出されます
        /// </summary>
		protected override void WriteFile()
		{
#if USE_JSONDATA && UNITY_STANDALONE
            File.WriteAllText( FilePath, m_JsonData );
#else
            File.WriteAllBytes( FilePath, m_ByteData );
#endif
		}

		/// <summary>
		/// 読み込み処理
		/// ファイルからデータを読み込みます
		/// 読み込みに失敗した場合はnullを返却します
		/// </summary>
		/// <returns>読み込んだデータ</returns>
		/// <typeparam name="T">保存したいセーブのクラス</typeparam>
		override public T Read<T>() where T : class
        {
            T data = null;

#if USE_JSONDATA && UNITY_STANDALONE
            try {
                string jsonText = File.ReadAllText( FilePath );
                data = JsonUtility.FromJson<T>( jsonText );
            } catch( Exception e ) {
                Debug.LogError( $"[PcSaveSystem.Load]ロードに失敗:\r\n{e.Message}" );
            }
#else
            try {
                byte[] bytes = File.ReadAllBytes( FilePath );
                data = Bytes2Struct<T>( bytes );
            } catch( Exception e ) {
                Debug.LogError( $"[PcSaveSystem.Load]ロードに失敗:\r\n{e.Message}" );
            }
#endif
            return data;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("SaveData/削除")]
        public static void DeleteSave()
        {
            if( File.Exists( FilePath ) ) {
                File.Delete( FilePath );
            }
        }

        const string kJsonSaveDefine = "USE_JSONDATA";
        const string kMenuTitle = "SaveData/Jsonで保存";
        [UnityEditor.MenuItem(kMenuTitle)]
        public static void ChangeSaveFormat()
        {
            bool isChecked=false;
            string[] defines = EditorUserBuildSettings.activeScriptCompilationDefines;
            if( defines.Contains( kJsonSaveDefine ) ) {
                DeleteSymbol( kJsonSaveDefine );
                isChecked = false;
            } else {
                AddSymbol( kJsonSaveDefine );
                isChecked = true;
            }
            Menu.SetChecked(kMenuTitle, isChecked);
        }

        [MenuItem(kMenuTitle, validate = true)]
        private static bool ChangeSaveFormatValidator()
        {
            string[] defines = EditorUserBuildSettings.activeScriptCompilationDefines;
            Menu.SetChecked(kMenuTitle, defines.Contains(kJsonSaveDefine));
            return true;
        }

        private static void DeleteSymbol( string str_target )
        {
            var symbols = GetSymbolList();
            if( symbols.Contains( str_target ) ){
                symbols.Remove( str_target );
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", symbols.ToArray()) );
            }
        }

        private static void AddSymbol( string str_target )
        {
            var symbols = GetSymbolList();
            if( !symbols.Contains( str_target ) ){
                symbols.Insert( 0, str_target );
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", symbols.ToArray()) );
            }
        }

        private static List<string> GetSymbolList()
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup ).Split( ';' ).Select( s => s.Trim() ).ToList();
        }
#endif
    }
}
#endif // UNITY_EDITOR || UNITY_STANDALONE