
using System;
using System.Diagnostics;

namespace ArchitectHS.AddressableManage
{
    internal static class Verbose
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Exception
        }
        
        private static LogLevel _currentLogLevel = LogLevel.Exception;
        
        public static void SetLogLevel(LogLevel level)
        {
            _currentLogLevel = level;
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void D(string message)
        {
            if (_currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"<color=#3498db>[ARM]</color> {message}");
            }
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void D(string message, string hexColor)
        {
            if (_currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"<color=#3498db>[ARM]</color> <color={hexColor}>{message}</color>");
            }
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void DFormat(string format, params object[] args)
        {
            if (_currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.LogFormat($"<color=#3498db>[ARM]</color> {format}", args);
            }
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void W(string message)
        {
            if (_currentLogLevel >= LogLevel.Warning)
            {
                UnityEngine.Debug.LogWarning($"<color=#f39c12>[ARM-W]</color> {message}");
            }
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void WFormat(string format, params object[] args)
        {
            if (_currentLogLevel >= LogLevel.Warning)
            {
                UnityEngine.Debug.LogWarningFormat($"<color=#f39c12>[ARM-W]</color> {format}", args);
            }
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void E(string message)
        {
            if (_currentLogLevel >= LogLevel.Error)
            {
                UnityEngine.Debug.LogError($"<color=#e74c3c>[ARM-E]</color> {message}");
            }
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void E(string format, params object[] args)
        {
            if (_currentLogLevel >= LogLevel.Error)
            {
                UnityEngine.Debug.LogErrorFormat($"<color=#e74c3c>[ARM-E]</color> {format}", args);
            }
        }
        
        [Conditional("ARM_DEBUGGING")]
        public static void Ex(Exception exception)
        {
            if (_currentLogLevel >= LogLevel.Exception)
            {
                UnityEngine.Debug.LogException(exception);
            }
        }

        [Conditional("ARM_DEBUGGING")]
        public static void Ex(string message, Exception exception)
        {
            if (_currentLogLevel >= LogLevel.Exception)
            {
                UnityEngine.Debug.LogError($"<color=#9b59b6>[ARM-EX]</color> {message}\n{exception}");
            }
        }

        [Conditional("ARM_DEBUGGING")]
        public static void DIf(bool condition, string message)
        {
            if (condition && _currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"<color=#3498db>[ARM-Conditional]</color> {message}");
            }
        }
    }
}