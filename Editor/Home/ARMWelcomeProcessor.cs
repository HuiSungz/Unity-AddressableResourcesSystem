
using UnityEditor;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Processor class that handles showing welcome window
    /// </summary>
    [InitializeOnLoad]
    public class ARMWelcomeProcessor
    {
        static ARMWelcomeProcessor()
        {
            // 도메인 리로드 후 실행
            EditorApplication.delayCall += ShowWelcomeWindowIfNeeded;
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