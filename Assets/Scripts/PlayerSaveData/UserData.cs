using curry.Common;
using curry.Enums;
using curry.Sound;
using curry.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace curry.UserData
{
    /// <summary>
    /// セーブデータ本体
    /// </summary>
    [System.Serializable,StructLayout(LayoutKind.Sequential)]
    public class SaveData
    {
        public HeaderData   m_Header;   // システム用ヘッダ
        public OptionData   m_Option;   // オプション
        public PlayData     m_UserData; // プレイデータ

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            m_Header = new HeaderData();
            m_Header.Initialize();

            m_Option = new OptionData();
            m_Option.Initialize();

            m_UserData = new PlayData();
            m_UserData.Initialize();
        }
    }

    /// <summary>
    /// システム用データ
    /// </summary>
    [System.Serializable,StructLayout(LayoutKind.Sequential)]
    public struct HeaderData
    {
        private const int kBufferSize = 256;

        [MarshalAs(UnmanagedType.I4)]
        public int      m_Version;   // セーブデータのバージョン
        [MarshalAs(UnmanagedType.ByValArray,SizeConst =kBufferSize),HideInInspector]
        public int[]    m_Buffer;    // 拡張用のバッファ

        public void Initialize()
        {
            m_Version = Constants.kSaveDataVersion;
            m_Buffer = new int[kBufferSize];
        }
    }

    /// <summary>
    /// オプションデータ
    /// </summary>
    [System.Serializable,StructLayout(LayoutKind.Sequential)]
    public struct OptionData
    { 
        [MarshalAs(UnmanagedType.I4)]
        public int m_EnvVolume;     // BGM音量
        [MarshalAs(UnmanagedType.I4)]
        public int m_SEVolume;      // SE音量
        [MarshalAs(UnmanagedType.U2)]
        public ScreenMode m_ScreenMode;    // スクリーンモード（フルスクリーン/ウィンドウ）
        [MarshalAs(UnmanagedType.I4)]
        public LocalizationLocaleType m_Language;      // 言語

        public void Initialize()
        {
            // BGM
            m_EnvVolume = BgmManager.SplitMax;
            m_SEVolume  = SEPlayer.kSplitMax;

            // スクリーンモード
            m_ScreenMode = ScreenMode.Window;

            // デフォルト言語
            m_Language = LocalizationUtility.GetDefalutLocaleType();
        }
    }

    /// <summary>
    /// プレイ記録
    /// </summary>
    [System.Serializable, StructLayout(LayoutKind.Sequential)]
    public struct PlayData
    {
        // public  const int kStageMax = 30*3; /// ステージ最大数（拡張性を持たせるために想定より多めに取っておく）

        // [MarshalAs(UnmanagedType.U8)]
        // public ulong m_LastSelectCharacterId;                       /// 最後に使用したキャラクターID

        // [MarshalAs(UnmanagedType.ByValArray,SizeConst =kCharaMax)]
        // public ulong[] m_SelectableCharacterIds;                    /// 使用可能なキャラクターIDリスト

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            InitializeArray();
        }

        /// <summary>
        /// 配列数などがあっていなければ補正する（基本的には開発時用の補正）
        /// </summary>
        public void InitializeArray()
        {
            // if (null == m_SelectableCharacterIds || kCharaMax > m_SelectableCharacterIds.Length)
            // {
            //     m_SelectableCharacterIds = new ulong[kCharaMax];
            // }
        }

#region データ更新

        //
        // /// <summary>
        // /// 倒した敵の数を取得
        // /// </summary>
        // /// <param name="achieveId">敵Id</param>
        // public uint GetKillEnemyCount( ulong achieveId )
        // {
        //     int index = Array.FindIndex( m_EnemyKillInfo, x => x.m_Id == achieveId );
        //     if( 0 > index ) {
        //         return 0;
        //     }
        //     return m_EnemyKillInfo[index].m_KillCount;
        // }


#endregion // データ更新
    }
}