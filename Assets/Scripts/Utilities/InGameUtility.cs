using curry.Common;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Extensions;

namespace curry.Utilities
{
    public static class CurryUtility
    {
        public static string GetScoreText(long score)
        {
            var timeSpan = TimeSpan.FromSeconds(score);

            var daySuffix = timeSpan.Days > 1 ? "days" : "day";
            var day = timeSpan.Days > 0 ? $"{timeSpan.Days}{daySuffix} " : string.Empty;

            var hour = $"{timeSpan.Hours:00}:";

            var minutes = $"{timeSpan.Minutes:00}:";

            var seconds = $"{timeSpan.Seconds:00}";

            return $"{day}{hour}{minutes}{seconds}";
        }
    }
}
