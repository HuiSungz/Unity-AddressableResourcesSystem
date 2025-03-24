
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
#if !ARM_UNITASK
        // 설치 상태를 추적하는 정적 필드
        private static bool _isSettingUpUniTask = false;
        
        static ARMDependencySetupProcessor()
        {
            // 도메인 리로드 후 실행
            EditorApplication.delayCall += SetupUniTaskDependency;
        }
        
        /// <summary>
        /// UniTask 의존성 설정
        /// </summary>
        private static void SetupUniTaskDependency()
        {
            if (_isSettingUpUniTask)
                return;
                
            var presenter = new ARMUniTaskDependencyPresenter();
            
            // 모델 인스턴스 생성
            var model = new ARMUniTaskDependencyModel();
            
            // 어셈블리가 이미 로드되어 있고 심볼도 추가되어 있는지 확인
            if (model.IsUniTaskAssemblyLoaded() && model.IsArmUniTaskSymbolAdded())
            {
                Debug.Log("ARM: UniTask is already properly setup.");
                return;
            }
                
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
                    Debug.LogWarning("ARM: UniTask setup in progress but not yet complete.");
                    Debug.LogWarning("ARM: The setup will continue next time Unity is started.");
                }
            });
        }
#endif
    }
}