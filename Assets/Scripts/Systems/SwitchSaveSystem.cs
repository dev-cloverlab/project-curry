#if UNITY_SWITCH
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using nn;
using System.Threading; 
using System.IO;
using Cysharp.Threading.Tasks;

namespace curry.Common
{
    /// <summary>
    /// 汎用セーブ処理（NSW）
    /// </summary>
    public class SwitchSaveSystem : ISaveSystem
    {
        private nn.account.Uid userId;
        private nn.fs.FileHandle fileHandle = new nn.fs.FileHandle();

        private const string mountName = "curry";         // プロジェクト名などを入れておく
        private const string fileName = "UserData";     // セーブファイル名
        private static readonly string filePath = string.Format("{0}:/{1}", mountName, fileName);

        /// <summary>
        /// 初期化
        /// </summary>
        public SwitchSaveSystem()
        {
            // ユーザーの取得
            nn.account.Account.Initialize();    // TODO 他の場所でやったほうがいい？
            nn.account.UserHandle userHandle = new nn.account.UserHandle();

            if (!nn.account.Account.TryOpenPreselectedUser(ref userHandle))
            {
                nn.Nn.Abort("Failed to open preselected user.");
            }
            nn.Result result = nn.account.Account.GetUserId(ref userId, userHandle);
            result.abortUnlessSuccess();

            // セーブをマウント
            result = nn.fs.SaveData.Mount(mountName, userId);
            result.abortUnlessSuccess();
        }

        /// <summary>
        /// ファイルが存在しているか
        /// </summary>
        /// <returns></returns>
        override public bool IsExistFile()
        {
#if false   // TODO : こっちのチェックも必要か？
            if( null == userId ) {
                DebugLogWrapper.LogError("[SwtichSaveSystem.IsExistFile]ユーザーを取得できていません");
                return false;
            }
            return nn.fs.SaveData.IsExisting( userId );
#endif

            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
            return result.IsSuccess();

        }

        /// <summary>
        /// 書き込み処理
        /// </summary>
        /// <param name="data">セーブしたいデータ</param>
        /// <typeparam name="T">保存したいセーブのクラス</typeparam>
        override public async UniTask<bool> Save<T>( T data ) where T : class
        {
            // UnityEngine.Switch.Notification.EnterExitRequestHandlingSection を呼びたい関係で定義しなおす

            if( m_Status != Status.Idle ) {
                return false; // 処理中なので実行しない
            }

            m_ByteData = Struct2Bytes<T>( data );
            if( null == m_ByteData || 0 == m_ByteData.Length ) {
                return false; // 書き込むものがない
            }

            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
            m_Status = Status.Processing;
            await UniTask.WaitWhile( () => m_Status!=Status.Idle );
            return true;
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
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
            if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
            {
                return default(T);
            }
            result.abortUnlessSuccess();

            result = nn.fs.File.Open(ref fileHandle, filePath, nn.fs.OpenFileMode.Read);
            result.abortUnlessSuccess();

            long fileSize = 0;
            result = nn.fs.File.GetSize(ref fileSize, fileHandle);
            result.abortUnlessSuccess();

            byte[] bytes = new byte[fileSize];  // TODO 毎回生成するのは無駄なのでどこかで常駐させる？
            result = nn.fs.File.Read(fileHandle, 0, bytes, fileSize);
            result.abortUnlessSuccess();

            nn.fs.File.Close(fileHandle);

            T data = Bytes2Struct<T>( bytes );
            return data;
        }

        /// <summary>
        /// ファイル書き込み実体
        /// Thread内で呼び出し
        /// </summary>
        override protected void WriteFile()
        {
            InitializeSaveData();   // データがなければ作成する TODO：書き込み時に毎回このチェックが走るのが重たい場合は処理を外へ逃がす

            nn.Result result = nn.fs.File.Open(ref fileHandle, filePath, nn.fs.OpenFileMode.Write);
            result.abortUnlessSuccess();

            const int offset = 0;
            result = nn.fs.File.Write(fileHandle, offset, m_ByteData, m_ByteData.LongLength, nn.fs.WriteOption.Flush);
            result.abortUnlessSuccess();

            nn.fs.File.Close(fileHandle);
            result = nn.fs.FileSystem.Commit(mountName);
            result.abortUnlessSuccess();
        }

        /// <summary>
        /// 処理終了時の後始末
        /// </summary>
		override protected void Done()
		{
			base.Done();
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
		}

		/// <summary>
		/// 解放処理
		/// </summary>
		override public void Release()
        {
            base.Release();
            if( m_Status != Status.Idle ) {
                UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
            }
            nn.fs.FileSystem.Unmount(mountName);
        }

#region 固有処理
        /// <summary>
        /// セーブデータの初期化
        /// ファイルが存在しない場合に作成を行います
        /// </summary>
        private void InitializeSaveData()
        {
            nn.fs.EntryType entryType = 0;
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, filePath);
            if (result.IsSuccess())
            {
                return;
            }
            if (!nn.fs.FileSystem.ResultPathNotFound.Includes(result))
            {
                result.abortUnlessSuccess();
            }

            result = nn.fs.File.Create(filePath, m_ByteData.Length);
            result.abortUnlessSuccess();
        }
#endregion 固有処理
    }
}
#endif
