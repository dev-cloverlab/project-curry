using System;
using System.ComponentModel;
using UnityEngine;

public static class EnumExtensions
{
    /// <summary>
    /// 名前の取得
    /// Description属性を持っているEnumはそちらの名前を優先して取得します
    /// なければ単純にToStringを返します
    /// </summary>
    /// <typeparam name="T">Enum</typeparam>
    /// <param name="kind">取得したい値</param>
    /// <returns>Enum名</returns>
    public static string GetName<T>( T kind ) where T : Enum
    {
        var info = kind.GetType().GetMember( kind.ToString() );
        if( null != info && 0 < info.Length ) {
            var attr = info[0].GetCustomAttributes( typeof(DescriptionAttribute), false ) as DescriptionAttribute[];
            if( null != attr && 0 < attr.Length ) {
                return attr[0].Description;
            }
        }
        return kind.ToString();
    }
}
