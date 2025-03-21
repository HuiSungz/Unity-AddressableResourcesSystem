using System;
using UnityEditor;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Presenter class for ARM Debug Settings Window
    /// Handles connection between Model and View
    /// </summary>
    public class ARMDebugPresenter
    {
        // Model instance
        private readonly ARMDebugModel _model;
        
        // Event handler
        public event Action OnStateChanged;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ARMDebugPresenter()
        {
            // Create model and connect events
            _model = new ARMDebugModel();
            _model.OnDebugStateChanged += HandleModelStateChanged;
        }
        
        /// <summary>
        /// Handle state change event from model
        /// </summary>
        private void HandleModelStateChanged(bool newState)
        {
            // Propagate event to View
            OnStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Check if debugging is enabled
        /// </summary>
        public bool IsDebuggingEnabled => _model.IsDebuggingEnabled;
        
        /// <summary>
        /// Get current target platform
        /// </summary>
        public BuildTargetGroup CurrentTargetGroup => _model.CurrentTargetGroup;
        
        /// <summary>
        /// Change target platform
        /// </summary>
        public void ChangeTargetGroup(BuildTargetGroup targetGroup)
        {
            _model.ChangeTargetGroup(targetGroup);
            OnStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Toggle debugging mode
        /// </summary>
        public void ToggleDebuggingMode()
        {
            _model.SetDebuggingEnabled(!_model.IsDebuggingEnabled);
        }
        
        /// <summary>
        /// Set debugging mode
        /// </summary>
        public void SetDebuggingEnabled(bool enabled)
        {
            _model.SetDebuggingEnabled(enabled);
        }
        
        /// <summary>
        /// Refresh current state
        /// </summary>
        public void RefreshState()
        {
            _model.RefreshDebugState();
            OnStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Get all current define symbols
        /// </summary>
        public string GetCurrentDefineSymbols()
        {
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(CurrentTargetGroup);
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
        }
        
        /// <summary>
        /// Get array of all current define symbols
        /// </summary>
        public string[] GetCurrentDefineSymbolsArray()
        {
            string symbols = GetCurrentDefineSymbols();
            return string.IsNullOrEmpty(symbols) 
                ? Array.Empty<string>() 
                : symbols.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}