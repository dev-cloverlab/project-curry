using UnityEngine;

namespace Extensions
{
    public static class FloatExtentions
    {
        public static float Round(this float self, int decimals = 0)
        {
            var keta = decimals > 0 ? Mathf.Pow(10, decimals) : 1f;
            return Mathf.Round(self * keta) / keta;
        }

        public static float Floor(this float self, int decimals = 0)
        {
            var keta = decimals > 0 ? Mathf.Pow(10, decimals) : 1f;
            return Mathf.Floor(self * keta) / keta;
        }

        public static float Ceil(this float self, int decimals = 0)
        {
            var keta = decimals > 0 ? Mathf.Pow(10, decimals) : 1f;
            return Mathf.Ceil(self * keta) / keta;
        }

        public static float ToPercent(this float self)
        {
            return self * 100f;
        }
    }
}