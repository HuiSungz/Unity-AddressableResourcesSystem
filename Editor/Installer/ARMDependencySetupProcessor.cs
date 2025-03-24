
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
        static ARMDependencySetupProcessor()
        {
            // Delay execution to ensure everything is properly loaded
            EditorApplication.delayCall += () =>
            {
                SetupUniTaskDependency();
                ShowWelcomeWindowIfNeeded();
            };
        }
        
        /// <summary>
        /// Setup UniTask dependency automatically
        /// </summary>
        private static void SetupUniTaskDependency()
        {
            var presenter = new ARMUniTaskDependencyPresenter();
            
            // Check if UniTask is properly setup
            if (!presenter.IsUniTaskProperlySetup())
            {
                Debug.Log("ARM: Setting up UniTask dependency...");
                
                // Setup UniTask completely
                bool setupSuccess = presenter.SetupUniTaskCompletely();
                
                if (setupSuccess)
                {
                    Debug.Log("ARM: UniTask dependency setup completed successfully.");
                    Debug.Log("ARM: Please wait for Unity to reload packages (this might take a moment).");
                }
                else
                {
                    Debug.LogError("ARM: Failed to setup UniTask dependency. Please install UniTask manually from OpenUPM.");
                }
            }
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