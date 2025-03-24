
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
        /// Check if UniTask is properly setup (manifest 기준)
        /// </summary>
        public bool IsUniTaskProperlySetup()
        {
            return _model.IsUniTaskInstalled() && _model.IsArmUniTaskSymbolAdded();
        }
        
        /// <summary>
        /// Check if UniTask is installed (manifest 기준)
        /// </summary>
        public bool IsUniTaskInstalled()
        {
            return _model.IsUniTaskInstalled();
        }
        
        /// <summary>
        /// Install UniTask from OpenUPM (manifest 업데이트만)
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
        /// Setup UniTask completely (install package and add symbol) - 동기 버전
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
        
        /// <summary>
        /// UniTask를 설치하고 설치가 완료된 후에 심볼 추가 - 비동기 버전
        /// </summary>
        public void SetupUniTaskCompletelyAsync(Action<bool> onComplete)
        {
            // 이미 설치되어 있는지 확인 (실제 패키지 확인)
            _model.CheckUniTaskInstallationAsync((isInstalled) => {
                if (isInstalled)
                {
                    // 이미 설치되어 있으면 심볼만 추가
                    if (!_model.IsArmUniTaskSymbolAdded())
                    {
                        _model.AddArmUniTaskSymbol();
                        OnUniTaskSymbolChanged?.Invoke(true);
                    }
                    onComplete?.Invoke(true);
                }
                else
                {
                    // 설치되어 있지 않으면 설치 진행
                    _model.InstallUniTaskPackageAsync((installSuccess) => {
                        if (installSuccess)
                        {
                            // 설치가 완료되면 심볼 추가
                            if (!_model.IsArmUniTaskSymbolAdded())
                            {
                                _model.AddArmUniTaskSymbol();
                                OnUniTaskSymbolChanged?.Invoke(true);
                            }
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            // 설치 실패
                            Debug.LogError("Failed to install UniTask package.");
                            onComplete?.Invoke(false);
                        }
                    });
                }
            });
        }
    }
}