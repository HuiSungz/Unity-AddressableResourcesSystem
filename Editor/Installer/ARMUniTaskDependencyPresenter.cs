
using System;
using UnityEngine;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Presenter class for UniTask dependency handling
    /// </summary>
    public class ARMUniTaskDependencyPresenter
    {
        private readonly ARMUniTaskDependencyModel _model;
        
        // Events
        public event Action<bool> OnUniTaskInstallationChanged;
        public event Action<bool> OnUniTaskSymbolChanged;
        
        public ARMUniTaskDependencyPresenter()
        {
            _model = new ARMUniTaskDependencyModel();
        }
        
        /// <summary>
        /// Check if UniTask is properly setup
        /// </summary>
        public bool IsUniTaskProperlySetup()
        {
            return _model.IsUniTaskInstalled() && _model.IsArmUniTaskSymbolAdded();
        }
        
        /// <summary>
        /// Check if UniTask is installed
        /// </summary>
        public bool IsUniTaskInstalled()
        {
            return _model.IsUniTaskInstalled();
        }
        
        /// <summary>
        /// Install UniTask from OpenUPM
        /// </summary>
        public bool InstallUniTask()
        {
            bool success = true;
            
            // Make sure OpenUPM registry is configured
            if (!_model.IsOpenUPMRegistryConfigured())
            {
                success = _model.AddOpenUPMRegistry();
                if (!success)
                {
                    Debug.LogError("Failed to configure OpenUPM registry.");
                    OnUniTaskInstallationChanged?.Invoke(false);
                    return false;
                }
            }
            
            // Add UniTask package
            if (!_model.IsUniTaskInstalled())
            {
                success = _model.AddUniTaskPackage();
                if (!success)
                {
                    Debug.LogError("Failed to add UniTask package.");
                    OnUniTaskInstallationChanged?.Invoke(false);
                    return false;
                }
            }
            
            OnUniTaskInstallationChanged?.Invoke(true);
            return true;
        }
        
        /// <summary>
        /// Check if ARM_UNITASK define symbol is added
        /// </summary>
        public bool IsArmUniTaskSymbolAdded()
        {
            return _model.IsArmUniTaskSymbolAdded();
        }
        
        /// <summary>
        /// Add ARM_UNITASK define symbol to all build target groups
        /// </summary>
        public void AddArmUniTaskSymbol()
        {
            _model.AddArmUniTaskSymbol();
            OnUniTaskSymbolChanged?.Invoke(true);
        }
        
        /// <summary>
        /// Setup UniTask completely (install package and add symbol)
        /// </summary>
        public bool SetupUniTaskCompletely()
        {
            // Install UniTask if needed
            if (!_model.IsUniTaskInstalled())
            {
                bool installSuccess = InstallUniTask();
                if (!installSuccess)
                {
                    return false;
                }
            }
            
            // Add define symbol if needed
            if (!_model.IsArmUniTaskSymbolAdded())
            {
                _model.AddArmUniTaskSymbol();
                OnUniTaskSymbolChanged?.Invoke(true);
            }
            
            return true;
        }
    }
}