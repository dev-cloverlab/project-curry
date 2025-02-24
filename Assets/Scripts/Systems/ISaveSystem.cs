using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using System.Threading; 
using Cysharp.Threading.Tasks; 

namespace curry.Common
{
    /// <summary>
    /// セーブ処理実体基底
    /// 各PFごとにこのクラスを継承して中身を実装
    /// エラーハンドリング・仕様の観点からファイルを複数分割して保存することは想定していない
    /// </summary>
    public abstract class ISaveSystem
    {
        /// <summary>
        /// 処理ステータス
        /// </summary>
        protected enum Status
        {
            Idle,       /// 待機
            Processing, /// 処理中
            Done,       /// 完了
        }

#if USE_JSONDATA && UNITY_STANDALONE
        protected string  m_JsonData  = string.Empty; /// 書き込み用実データ
#else
        protected byte[]  m_ByteData  = null;         /// 書き込み用実データ
#endif

        private   Thread  m_Thread    = null;         /// 処理スレッド
        private   bool    m_IsRunnning= false;        /// スレッド実行中フラグ
        protected Status  m_Status    = Status.Idle;  /// 現在のステータス
        public bool IsProcessing => Status.Idle != m_Status;

        /// <summary>
        /// セーブが存在しているか
        /// </summary>
        /// <returns></returns>
        public abstract bool IsExistFile();

        /// <summary>
        /// 読み込み処理
        /// ファイルからデータを読み込みます
        /// 読み込みに失敗した場合はnullを返却します
        /// </summary>
        /// <returns>読み込んだデータ</returns>
        /// <typeparam name="T">保存したいセーブのクラス</typeparam>
        public abstract T Read<T>() where T : class;

        /// <summary>
        /// 実際の書き込み処理実体
        /// Thread内で呼び出されます
        /// </summary>
        protected abstract void WriteFile();

        /// <summary>
        /// 書き込み
        /// </summary>
        /// <typeparam name="T">保存したいセーブのクラス</typeparam>
        /// <param name="data">セーブしたいデータ</param>
        public virtual async UniTask<bool> Save<T>( T data ) where T : class
        {
            if( m_Status != Status.Idle ) {
                return false; // 処理中なので実行しない
            }

#if USE_JSONDATA && UNITY_STANDALONE
            m_JsonData  = JsonUtility.ToJson( data, true );
            if( string.IsNullOrEmpty( m_JsonData ) ) {
                return false; // 書き込むものがない
            }
#else
            m_ByteData = Struct2Bytes<T>( data );
            if( null == m_ByteData || 0 == m_ByteData.Length ) {
                return false; // 書き込むものがない
            }
#endif

            m_Status = Status.Processing;
            await UniTask.WaitWhile( () => m_Status!=Status.Idle );
            return true;
        }

        /// 書き込み処理完了
        /// </summary>
        protected virtual void Done()
        {
            m_Status = Status.Idle;
        }

        /// <summary>
        /// 読み込み
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Load<T>() where T : class
        {
            return default( T );
        }

        /// <summary>
        /// 解放処理
        /// Threadの終了待ち・終了
        /// </summary>
        public virtual void Release()
        {
            JoinThread();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public ISaveSystem()
        {
            StartThread();
        }

        /// <summary>
        /// 処理監視
        /// </summary>
        public void Update()
        {
            switch( m_Status )
            { 
            case Status.Processing:
                // Threadに任せるので処理しない
                break;

            case Status.Done:
                // Update内で処理しているのはSwicthの後始末がメインThread出ないと呼び出せないため
                Done();
                break;

             default:
                break;
            }
        }

        /// <summary>
        /// スレッドループ
        /// </summary>
        private void ThreadLoop()
        {
            while (m_IsRunnning)
            {
                if (m_Status == Status.Processing)
                {
                    WriteFile();
                    m_Status = Status.Done;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// 常駐スレッドの開始
        /// ゲーム起動～ゲーム終了まで常駐する
        /// </summary>
        private void StartThread()
        {
            if( null != m_Thread ) {
                DebugLogWrapper.LogAssertion( "Threadがすでに存在している" );
                return;
            }
            m_Thread = new Thread(ThreadLoop);
            m_Thread.Priority = System.Threading.ThreadPriority.Lowest;
            m_IsRunnning = true;
            m_Thread.Start();
        }

        /// <summary>
        /// スレッドの終了待ち
        /// </summary>
        private void JoinThread()
        {
            if( null == m_Thread ) {
                DebugLogWrapper.LogAssertion( "Threadが存在しない" );
                return;
            }
            m_IsRunnning = false;
            m_Thread.Join();
            m_Thread = null;
        }

#region 汎用処理
        /// <summary>
        /// バイナリ⇒構造体へ変換
        /// リストを含んだクラスは指定できません
        /// 配列にして固定長にしたものを渡してください
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected byte[] Struct2Bytes<T>( T obj ) where T : class
        {
            int size = Marshal.SizeOf( typeof(T) );

            IntPtr ptr = Marshal.AllocHGlobal( size );
            Marshal.StructureToPtr( obj, ptr, false );
 
            byte[] bytes = new byte[size];
            Marshal.Copy( ptr, bytes, 0, size );
            Marshal.FreeHGlobal( ptr );

            return bytes;
        }

        /// <summary>
        /// 構造体⇒バイナリへ変換
        /// リストを含んだクラスは指定できません
        /// 配列にして固定長にしたものを渡してください
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected T Bytes2Struct<T>( byte[] bytes ) where T : class
        {
            var gch = GCHandle.Alloc( bytes, GCHandleType.Pinned );
            var result = (T)Marshal.PtrToStructure( gch.AddrOfPinnedObject(), typeof(T) );
            gch.Free();
            return result;
        }
#endregion 汎用処理
    }
}