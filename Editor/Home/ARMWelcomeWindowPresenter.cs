
using System;
using UnityEngine;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Presenter class for ARM Welcome Window
    /// Handles connection between Model and View
    /// </summary>
    public class ARMWelcomeWindowPresenter
    {
        private readonly ARMWelcomeWindowModel _model;

        public event Action OnWindowClosed;

        public ARMWelcomeWindowPresenter()
        {
            _model = new ARMWelcomeWindowModel();
        }

        /// <summary>
        /// Check if welcome window should be shown
        /// </summary>
        public bool ShouldShowWelcomeWindow()
        {
            return _model.ShouldShowWelcomeWindow();
        }

        /// <summary>
        /// Set "Don't show again" preference
        /// </summary>
        public void SetDontShowAgain(bool dontShow)
        {
            _model.SetDontShowAgain(dontShow);
        }

        /// <summary>
        /// Get title image from Resources
        /// </summary>
        public Texture2D GetTitleImage()
        {
            return _model.GetTitleImage();
        }

        /// <summary>
        /// Open GitHub repository URL
        /// </summary>
        public void OpenRepositoryURL()
        {
            _model.OpenRepositoryURL();
        }

        /// <summary>
        /// Close the window
        /// </summary>
        public void CloseWindow(bool dontShowAgain)
        {
            SetDontShowAgain(dontShowAgain);
            OnWindowClosed?.Invoke();
        }
    }
}