using curry.Scriptable;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using curry.UserData;
using curry.Sound;
using curry.Enums;
using curry.Utilities;

namespace curry.Common
{
    /// <summary>
    /// 常駐のゲームデータ
    /// 起動時からゲーム終了まで常駐します
    /// </summary>
    public class UserDataManager : SingletonMonoBehaviour<UserDataManager>
    {
        private SaveData m_playData = new SaveData();    /// <summary> 現在のプレイデータ </summary>
        public ref SaveData Data => ref m_playData;
        public ref OptionData Option {
            get { return ref m_playData.m_Option; }
        }
        public ref UserData.PlayData PlayData {
            get { return ref m_playData.m_UserData; }
        }

        /// <summary>
        /// データのセット
        /// ロード時に呼び出されます
        /// ロード時に必要な処理を定義
        /// </summary>
        /// <param name="data"></param>
        public void Set( SaveData data )
        {
            // TODO : memcopy的なものをするべきか？
            m_playData = data;
            m_playData.m_UserData.InitializeArray();

            // 音量の反映
            BgmManager.Instance.Player.VolumeSet(m_playData.m_Option.m_BGMVolume);
            EnvSoundManager.Instance.Player.VolumeSet(m_playData.m_Option.m_BGMVolume);
            FireSoundManager.Instance.Player.VolumeSet(m_playData.m_Option.m_BGMVolume);
            SEPlayer.VolumeSet(m_playData.m_Option.m_SEVolume);

            // ウィンドウ設定
            ScreenUtility.SetScreenMode( m_playData.m_Option.m_ScreenMode );
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private new void Awake()
        {
            base.Awake();
            if( !IsThisInstance() ) {
                return;
            }
            m_playData = new SaveData();
            m_playData.Initialize();
            DontDestroyOnLoad();
        }
    }
}
