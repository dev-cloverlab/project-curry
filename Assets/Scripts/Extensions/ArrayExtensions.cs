using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    /// <summary>
    /// 指定された要素の配列をシャッフルする
    /// </summary>
    public static void Shuffle<T>(this IList<T> array)
    {
        for (var i = array.Count - 1; i > 0; --i)
        {
            // 0以上i以下のランダムな整数を取得
            // Random.Rangeの最大値は第２引数未満なので、+1することに注意
            var j = Random.Range(0, i + 1);

            // i番目とj番目の要素を交換する
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}