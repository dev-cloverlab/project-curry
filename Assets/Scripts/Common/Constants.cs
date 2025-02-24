using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

namespace curry.Common
{
    public static class Constants
    {
        /// <summary> セーブデータのバージョン番号 </summary>
        public const int kSaveDataVersion = 1;

        /// <summary> ゲームのFPS </summary>
        public const int kGameFps = 60;

        /// <summary> 一万分率計算用 </summary>
        public const int kPerRatioCoefficient = 10000;

        /// <summary> ダメージテキストカラー </summary>
        public const string kPlayerDamageTextColor = "#FF2E2E";
        public const string kHealTextColor = "#13FFD5";
        public const string kEnemyDamageTextColor1 = "#FFFFFF";
        public const string kEnemyDamageTextColor2 = "#FFF000";
        public const string kEnemyDamageTextColor3 = "#FF98D0";
        public const string kEnemyDamageTextColor4 = "#D800FF";

        /// <summary> 強調テキストカラー </summary>
        public const string kHighlightColor = "#69FCCE";

        // メモリ使用量の観点からColorは使わずColor32を使用する
        // https://xrdnk.hateblo.jp/entry/2020/09/05/153149
        public static readonly Color32 kColorWhite = new(255, 255, 255, 255);
        public static readonly Color32 kColorBlack = new(0, 0, 0, 255);
        public static readonly Color32 kColorGray = new(128, 128, 128, 255);
        public static readonly Color32 kColorRed = new(255, 0, 0, 255);
        public static readonly Color32 kColorClear = new(0, 0, 0, 0);
        public static readonly Color32 kColorBlue = new(0, 0, 255, 255);

        // ===================
        // Layer
        // ===================

        // ===================
        // Tags
        // ===================

        // ===================
        // SortingLayer
        // ===================

        // ===================
        // AnimationHash
        // ===================
    }
}