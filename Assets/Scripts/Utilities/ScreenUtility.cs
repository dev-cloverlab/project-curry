using UnityEngine;
using curry.Enums;

namespace curry.Utilities
{
    public class ScreenUtility
    {
        public static readonly Vector2Int kWindowSize = new Vector2Int( 1280, 720 );
    
        /// <summary>
        /// 画面モードの設定
        /// </summary>
        /// <param name="windowMode"></param>
        [System.Diagnostics.Conditional("UNITY_STANDALONE")]
        public static void SetScreenMode( ScreenMode windowMode )
        {
            switch( windowMode )
            {
            case ScreenMode.Window:
                Screen.SetResolution( kWindowSize.x, kWindowSize.y, false );
                Screen.fullScreen = false;
                break;
            case ScreenMode.FullScreen:
                Screen.SetResolution( Screen.currentResolution.width, Screen.currentResolution.height, true );
                Screen.fullScreen = true;
                break;
            }
        }
    }
}
