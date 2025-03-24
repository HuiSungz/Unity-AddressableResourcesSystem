
using System;
using UnityEngine;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Presenter class for UniTask dependency handling
    /// </summary>
    public class ARMUniTaskDependencyPresenter
    {
#if !ARM_UNITASK
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
        /// UniTask를 설치하고 실제 어셈블리가 로드된 후에 심볼 추가 - 비동기 버전
        /// </summary>
        public void SetupUniTaskCompletelyAsync(Action<bool> onComplete)
        {
            // 이미 어셈블리가 로드되어 있는지 확인
            if (_model.IsUniTaskAssemblyLoaded())
            {
                Debug.Log("ARM: UniTask assembly is already loaded.");
                // 어셈블리가 로드되었으면 심볼 추가
                if (!_model.IsArmUniTaskSymbolAdded())
                {
                    _model.AddArmUniTaskSymbol();
                    OnUniTaskSymbolChanged?.Invoke(true);
                }
                onComplete?.Invoke(true);
                return;
            }
            
            // 패키지 설치 여부 확인
            _model.CheckUniTaskInstallationAsync((isInstalled) => {
                if (isInstalled)
                {
                    // 패키지는 설치되어 있지만 어셈블리가 로드되지 않은 경우
                    Debug.Log("ARM: UniTask package is installed but assembly is not loaded yet.");
                    
                    // 패키지가 로드될 때까지 기다림
                    WaitForUniTaskAssemblyLoaded();
                }
                else
                {
                    // 패키지가 설치되어 있지 않으면 설치 진행
                    Debug.Log("ARM: Installing UniTask package...");
                    _model.InstallUniTaskPackageAsync((installSuccess) => {
                        if (installSuccess)
                        {
                            Debug.Log("ARM: UniTask package installed successfully. Waiting for assembly to load...");
                            // 패키지 설치 후 어셈블리 로드 대기
                            WaitForUniTaskAssemblyLoaded();
                        }
                        else
                        {
                            // 설치 실패
                            Debug.LogError("ARM: Failed to install UniTask package.");
                            onComplete?.Invoke(false);
                        }
                    });
                }
            });
            
            // 어셈블리 로드 대기 함수
            void WaitForUniTaskAssemblyLoaded()
            {
                _model.CheckUniTaskAssemblyLoadedAsync((loaded) => {
                    if (loaded)
                    {
                        Debug.Log("ARM: UniTask assembly loaded successfully.");
                        // 어셈블리가 로드되었으면 심볼 추가
                        if (!_model.IsArmUniTaskSymbolAdded())
                        {
                            _model.AddArmUniTaskSymbol();
                            OnUniTaskSymbolChanged?.Invoke(true);
                        }
                        onComplete?.Invoke(true);
                    }
                    else
                    {
                        Debug.LogError("ARM: UniTask assembly failed to load in reasonable time.");
                        Debug.LogError("ARM: Please restart Unity to complete the setup.");
                        onComplete?.Invoke(false);
                    }
                });
            }
        }
#else
        public ARMUniTaskDependencyPresenter()
        {
            // ARM_UNITASK 심볼이 이미 정의되어 있으면 아무것도 하지 않음
            Debug.Log("ARM: UniTask is already installed and configured.");
        }
        
        // ARM_UNITASK 심볼이 정의된 경우 더미 메서드들
        public bool IsUniTaskProperlySetup() => true;
        public bool IsUniTaskInstalled() => true;
        public bool IsArmUniTaskSymbolAdded() => true;
        public bool InstallUniTask() => true;
        public void AddArmUniTaskSymbol() { }
        public bool SetupUniTaskCompletely() => true;
        public void SetupUniTaskCompletelyAsync(Action<bool> onComplete) => onComplete?.Invoke(true);
#endif
    }
}