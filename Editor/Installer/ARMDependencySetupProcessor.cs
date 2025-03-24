
using UnityEditor;
using UnityEngine;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Processor class that runs on domain reload to verify dependencies
    /// </summary>
    [InitializeOnLoad]
    public class ARMDependencySetupProcessor
    {
        // 설치 상태를 추적하는 정적 필드
        private static bool _isSettingUpUniTask = false;
        
        static ARMDependencySetupProcessor()
        {
            // 도메인 리로드 후 실행
            EditorApplication.delayCall += () =>
            {
                SetupUniTaskDependency();
                ShowWelcomeWindowIfNeeded();
            };
        }
        
        /// <summary>
        /// UniTask 의존성 설정
        /// </summary>
        private static void SetupUniTaskDependency()
        {
            if (_isSettingUpUniTask)
                return;
                
            var presenter = new ARMUniTaskDependencyPresenter();
            
            // 이미 설치 완료되었는지 확인 (manifest 기준)
            if (presenter.IsUniTaskProperlySetup())
                return;
                
            // 설치 진행 중임을 표시
            _isSettingUpUniTask = true;
            Debug.Log("ARM: Setting up UniTask dependency...");
            
            // 비동기로 설치 진행
            presenter.SetupUniTaskCompletelyAsync((success) => {
                _isSettingUpUniTask = false;
                
                if (success)
                {
                    Debug.Log("ARM: UniTask dependency setup completed successfully.");
                }
                else
                {
                    Debug.LogError("ARM: Failed to setup UniTask dependency. Please install UniTask manually from OpenUPM.");
                }
            });
        }
        
        /// <summary>
        /// Show welcome window if needed
        /// </summary>
        private static void ShowWelcomeWindowIfNeeded()
        {
            ARMWelcomeWindow.ShowWindowIfNeeded();
        }
    }
}