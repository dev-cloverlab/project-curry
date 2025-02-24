using System;
using System.ComponentModel;

namespace curry.Enums
{
    public enum LocalizationLocaleType
    {
        none,

        ja,

        en,
    }

#region オプション関連
    /// <summary>
    /// スクリーンモード種類
    /// </summary>
    public enum ScreenMode : ushort
    {
        FullScreen,
        Window
    }
#endregion オプション関連
}
