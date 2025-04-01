
using System;
using UnityEditor;

namespace ArchitectHS.AddressableManage.Editor
{
    public class ARMDebugModel
    {
        private const string ARM_DEBUGGING_SYMBOL = "ARM_DEBUGGING";
        private BuildTargetGroup _currentTargetGroup;
        private bool _isDebuggingEnabled;
        public event Action<bool> OnDebugStateChanged;
        
        public ARMDebugModel()
        {
            _currentTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            
            // Check current debugging state
            RefreshDebugState();
        }
        
        public bool IsDebuggingEnabled => _isDebuggingEnabled;
        
        public BuildTargetGroup CurrentTargetGroup => _currentTargetGroup;
        
        public void ChangeTargetGroup(BuildTargetGroup targetGroup)
        {
            if (_currentTargetGroup == targetGroup)
                return;
                
            _currentTargetGroup = targetGroup;
            RefreshDebugState();
        }
        
        public void RefreshDebugState()
        {
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(_currentTargetGroup);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            _isDebuggingEnabled = ContainsSymbol(symbols, ARM_DEBUGGING_SYMBOL);
        }
        
        public void SetDebuggingEnabled(bool enable)
        {
            if (_isDebuggingEnabled == enable)
                return;
                
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(_currentTargetGroup);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            
            if (enable)
            {
                // Add debugging symbol
                symbols = AddSymbol(symbols, ARM_DEBUGGING_SYMBOL);
            }
            else
            {
                // Remove debugging symbol
                symbols = RemoveSymbol(symbols, ARM_DEBUGGING_SYMBOL);
            }
            
            // Save symbols
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_currentTargetGroup, symbols);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            
            // Update state
            _isDebuggingEnabled = enable;
            
            // Trigger event
            OnDebugStateChanged?.Invoke(enable);
            
            // Save changes
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// Check if symbol string contains specific symbol
        /// </summary>
        private bool ContainsSymbol(string symbolString, string symbol)
        {
            string[] symbols = symbolString.Split(';', ',', ' ');
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i].Trim() == symbol)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Add new symbol to symbol string
        /// </summary>
        private string AddSymbol(string symbolString, string symbol)
        {
            if (string.IsNullOrEmpty(symbolString))
                return symbol;
                
            if (ContainsSymbol(symbolString, symbol))
                return symbolString;
                
            return symbolString + ";" + symbol;
        }
        
        /// <summary>
        /// Remove symbol from symbol string
        /// </summary>
        private string RemoveSymbol(string symbolString, string symbol)
        {
            if (string.IsNullOrEmpty(symbolString))
                return string.Empty;
                
            string[] symbols = symbolString.Split(';', ',', ' ');
            string result = "";
            
            for (int i = 0; i < symbols.Length; i++)
            {
                string current = symbols[i].Trim();
                if (string.IsNullOrEmpty(current) || current == symbol)
                    continue;
                    
                if (!string.IsNullOrEmpty(result))
                    result += ";";
                    
                result += current;
            }
            
            return result;
        }
    }
}