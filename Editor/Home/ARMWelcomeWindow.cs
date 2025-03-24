
using UnityEditor;
using UnityEngine;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Welcome window for ARM system
    /// </summary>
    public class ARMWelcomeWindow : EditorWindow
    {
        private ARMWelcomeWindowPresenter _presenter;
        
        private Texture2D _titleImage;
        private GUIStyle _headerStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _descriptionStyle;
        private GUIStyle _featureStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _featureBoxStyle;
        
        private bool _dontShowAgain;
        
        private Vector2 _scrollPosition;
        
        [MenuItem("ARM/Welcome Message", priority = 0)]
        public static void ShowWindow()
        {
            ARMWelcomeWindow window = GetWindow<ARMWelcomeWindow>("Welcome to ARM");
            window.minSize = new Vector2(750, 650);
            window.maxSize = new Vector2(750, 650);
            window.ShowUtility(); // 팝업 스타일로 표시
        }
        
        /// <summary>
        /// Show window if not disabled by preferences
        /// </summary>
        public static void ShowWindowIfNeeded()
        {
            var presenter = new ARMWelcomeWindowPresenter();
            if (presenter.ShouldShowWelcomeWindow())
            {
                ShowWindow();
            }
        }
        
        private void OnEnable()
        {
            _presenter = new ARMWelcomeWindowPresenter();
            _presenter.OnWindowClosed += Close;
            
            _titleImage = _presenter.GetTitleImage();
        }
        
        private void OnDisable()
        {
            if (_presenter != null)
            {
                _presenter.OnWindowClosed -= Close;
            }
        }
        
        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 22,
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(10, 10, 10, 10),
                    normal = { textColor = new Color(0.2f, 0.6f, 1.0f) }
                };
            }
            
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(5, 5, 10, 5),
                    normal = { textColor = new Color(0.8f, 0.8f, 0f) }
                };
            }
            
            if (_descriptionStyle == null)
            {
                _descriptionStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    fontSize = 12,
                    alignment = TextAnchor.UpperLeft,
                    margin = new RectOffset(10, 10, 5, 10),
                    normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
                };
            }
            
            if (_featureStyle == null)
            {
                _featureStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(10, 10, 2, 2),
                    richText = true
                };
            }
            
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 35,
                    margin = new RectOffset(5, 5, 5, 5)
                };
            }
            
            if (_toggleStyle == null)
            {
                _toggleStyle = new GUIStyle(EditorStyles.toggle)
                {
                    fontSize = 11,
                    margin = new RectOffset(5, 5, 5, 5)
                };
            }
            
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(15, 15, 15, 15),
                    margin = new RectOffset(10, 10, 10, 10)
                };
            }
            
            if (_featureBoxStyle == null)
            {
                _featureBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(10, 10, 5, 5)
                };
            }
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Welcome to Addressable Resources Management", _headerStyle);
            EditorGUILayout.Space(15);
            
            // Title Image
            if (_titleImage != null)
            {
                Rect imageRect = EditorGUILayout.GetControlRect(false, 350);
                imageRect.width = 450;
                imageRect.x = (position.width - imageRect.width) / 2;
                GUI.DrawTexture(imageRect, _titleImage, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUILayout.LabelField("(Title image not found in Resources/Title.png)", _descriptionStyle);
            }
            
            EditorGUILayout.Space(10);
            
            // Main Content Box
            EditorGUILayout.BeginVertical(_boxStyle);
            
            // Description
            EditorGUILayout.LabelField("About Addressable Resources Management", _titleStyle);
            
            EditorGUILayout.LabelField(
                "Addressables Resource Management is a powerful wrapper solution for Unity's Addressables system. " +
                "This package simplifies the complex Addressables API and streamlines your game development workflow, " +
                "optimizing memory usage and improving loading performance in large game projects.",
                _descriptionStyle
            );
            
            EditorGUILayout.Space(15);
            
            // Features Box
            EditorGUILayout.LabelField("Key Features", _titleStyle);
            
            EditorGUILayout.BeginVertical(_featureBoxStyle);
            
            EditorGUILayout.LabelField("<b>• AssetEntry System:</b> Provides safe and consistent access to all addressable assets", _featureStyle);
            EditorGUILayout.LabelField("<b>• Automatic Memory Management:</b> Auto-release through MonoBehaviourTracker when instantiating", _featureStyle);
            EditorGUILayout.LabelField("<b>• Synchronous and Asynchronous Instantiation:</b> Support for both Instantiate and InstantiateAsync methods", _featureStyle);
            EditorGUILayout.LabelField("<b>• Advanced Handle Tracking:</b> Prevents memory leaks with HandleTrackingSystem", _featureStyle);
            EditorGUILayout.LabelField("<b>• Efficient Asset Group Management:</b> Optimizes memory usage through proper asset group organization", _featureStyle);
            EditorGUILayout.LabelField("<b>• Unity 2023+ Support:</b> High-performance asynchronous instantiation using AsyncInstantiateOperation", _featureStyle);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(15);
            
            // Setup Information
            EditorGUILayout.LabelField("Getting Started", _titleStyle);
            
            EditorGUILayout.LabelField(
                "ARM automatically installs and configures UniTask from OpenUPM as a dependency. " +
                "The ARM_UNITASK define symbol will be added to all your build targets. " +
                "Check the documentation for detailed usage instructions and examples.",
                _descriptionStyle
            );
            
            EditorGUILayout.Space(15);
            
            // Buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Contact", _buttonStyle, GUILayout.Width(150)))
            {
                _presenter.OpenRepositoryURL();
            }
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Close", _buttonStyle, GUILayout.Width(150)))
            {
                _presenter.CloseWindow(_dontShowAgain);
            }
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Don't show again toggle
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _dontShowAgain = EditorGUILayout.Toggle("Don't show again", _dontShowAgain, _toggleStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.EndScrollView();
        }
    }
}