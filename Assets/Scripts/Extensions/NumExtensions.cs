using UnityEngine;

namespace Extensions
{
    public static class NumExtensions
    {
        public static int GetDigit(this int num)
        {
            num = System.Math.Abs(num);
            return (num == 0) ? 1 : ((int)System.Math.Log10(num) + 1);
        }
    }
}
