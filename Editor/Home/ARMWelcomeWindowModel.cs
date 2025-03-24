
using UnityEngine;
using UnityEditor;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Model class for ARM Welcome Window
    /// Handles data storage and preferences
    /// </summary>
    public class ARMWelcomeWindowModel
    {
        private const string DONT_SHOW_PREF_KEY = "ARM_Welcome_DontShowAgain";

        /// <summary>
        /// Check if welcome window should be shown
        /// </summary>
        public bool ShouldShowWelcomeWindow()
        {
            // Check if "Don't show again" preference is set
            return !EditorPrefs.GetBool(DONT_SHOW_PREF_KEY, false);
        }

        /// <summary>
        /// Set "Don't show again" preference
        /// </summary>
        public void SetDontShowAgain(bool dontShow)
        {
            EditorPrefs.SetBool(DONT_SHOW_PREF_KEY, dontShow);
        }

        /// <summary>
        /// Get title image from Resources
        /// </summary>
        public Texture2D GetTitleImage()
        {
            return Resources.Load<Texture2D>("Title");
        }

        /// <summary>
        /// Open GitHub repository URL
        /// </summary>
        public void OpenRepositoryURL()
        {
            Application.OpenURL("https://github.com/HuiSungz/Unity-AddressableResourcesSystem");
        }
    }
}