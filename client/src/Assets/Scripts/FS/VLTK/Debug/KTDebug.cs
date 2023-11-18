using System;
using UnityEngine;

/// <summary>
/// Lớp Debug
/// </summary>
public static class KTDebug
{
    /// <summary>
    /// Kích hoạt Debug
    /// </summary>
    public static bool EnableDebug { get; set; }

    /// <summary>
    /// Ghi LOG
    /// </summary>
    /// <param name="message"></param>
    public static void Log(string message)
    {
        if (!KTDebug.EnableDebug)
        {
            return;
        }
        Debug.Log(message);
    }

    /// <summary>
    /// Ghi LOG
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    public static void Log(string message, params object[] parameters)
    {
        if (!KTDebug.EnableDebug)
        {
            return;
        }
        string msg = string.Format(message, parameters);
        Debug.Log(msg);
    }

    /// <summary>
    /// Ghi LOG Warning
    /// </summary>
    /// <param name="message"></param>
    public static void LogWarning(string message)
    {
        if (!KTDebug.EnableDebug)
        {
            return;
        }
        Debug.LogWarning(message);
    }

    /// <summary>
    /// Ghi LOG Warning
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    public static void LogWarning(string message, params object[] parameters)
    {
        if (!KTDebug.EnableDebug)
        {
            return;
        }
        string msg = string.Format(message, parameters);
        Debug.LogWarning(msg);
    }

    /// <summary>
    /// Ghi LOG Warning
    /// </summary>
    /// <param name="message"></param>
    public static void LogError(string message)
    {
        if (!KTDebug.EnableDebug)
        {
            return;
        }
        Debug.LogError(message);
    }

    /// <summary>
    /// Ghi LOG Warning
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    public static void LogError(string message, params object[] parameters)
    {
        if (!KTDebug.EnableDebug)
        {
            return;
        }
        string msg = string.Format(message, parameters);
        Debug.LogError(msg);
    }

    /// <summary>
    /// Ghi LOG Warning
    /// </summary>
    /// <param name="ex"></param>
    public static void LogException(Exception ex)
    {
        if (!KTDebug.EnableDebug)
        {
            return;
        }
        else if (ex == null)
        {
            return;
        }
        Debug.LogException(ex);
    }
}