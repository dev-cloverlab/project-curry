using UnityEngine;

public static class DebugLogWrapper
{
    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Log(object message, Object context)
    {
        if(IsEnableDebugLog())
        {
            Debug.Log(message, context);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Log(object message)
    {
        if(IsEnableDebugLog())
        {
            Debug.Log(message);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogAssertion(object message, Object context)
    {
        if(IsEnableAssertion())
        {
            Debug.LogAssertion(message, context);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogAssertion(object message)
    {
        if(IsEnableAssertion())
        {
            Debug.LogAssertion(message);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogAssertionFormat(Object context, string format, params object[] args)
    {
        if(IsEnableAssertion())
        {
            Debug.LogAssertionFormat(context, format, args);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogAssertionFormat(string format, params object[] args)
    {
        if(IsEnableAssertion())
        {
            Debug.LogAssertionFormat(format, args);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogError(object message, Object context)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogError(message, context);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogError(object message)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogError(message);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogErrorFormat(string format, params object[] args)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogErrorFormat(format, args);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogErrorFormat(context, format, args);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogException(System.Exception exception, Object context)
    {
        if(IsEnableException())
        {
            Debug.LogException(exception, context);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogException(System.Exception exception)
    {
        if(IsEnableException())
        {
            Debug.LogException(exception);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogFormat(Object context, string format, params object[] args)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogFormat(context, format, args);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogFormat(string format, params object[] args)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogFormat(format, args);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogWarning(object message)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogWarning(message);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogWarning(object message, Object context)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogWarning(message, context);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogWarningFormat(string format, params object[] args)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogWarningFormat(format, args);
        }
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        if(IsEnableDebugLog())
        {
            Debug.LogWarningFormat(context, format, args);
        }
    }

    // Editorメニューなどでデバッグモード中もOnOffできるようにしたいのでガワだけ作っておく。
    private static bool IsEnableDebugLog()
    {
        return true;
    }

    private static bool IsEnableAssertion()
    {
        return true;
    }

    private static bool IsEnableException()
    {
        return true;
    }
}
