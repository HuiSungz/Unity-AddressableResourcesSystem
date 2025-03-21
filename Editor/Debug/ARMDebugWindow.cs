using UnityEditor;
using UnityEngine;

namespace AddressableManage.Editor
{
    /// <summary>
    /// Editor window for managing ARM debug settings
    /// </summary>
    public class ARMDebugWindow : EditorWindow
    {
        // Singleton instance
        private static ARMDebugWindow _instance;
        
        // Presenter instance
        private ARMDebugPresenter _presenter;
        
        // UI styles
        private GUIStyle _headerStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _descriptionStyle;
        private GUIStyle _symbolButtonStyle;
        private GUIStyle _symbolBoxStyle;
        
        // Scroll position for symbols list
        private Vector2 _symbolsScrollPosition;
        
        // Build target selection index
        private int _selectedBuildTargetIndex;
        private readonly string[] _buildTargetNames = new string[]
        {
            "Standalone", "iOS", "Android", "WebGL", "Windows Store"
        };
        private readonly BuildTargetGroup[] _buildTargetGroups = new BuildTargetGroup[]
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.iOS,
            BuildTargetGroup.Android,
            BuildTargetGroup.WebGL,
            BuildTargetGroup.WSA
        };
        
        /// <summary>
        /// Menu item to open the window
        /// </summary>
        [MenuItem("ARM/Debug Settings")]
        public static void ShowWindow()
        {
            _instance = GetWindow<ARMDebugWindow>("ARM Debug Settings");
            _instance.minSize = new Vector2(400, 450);
            _instance.maxSize = new Vector2(600, 450);
        }
        
        /// <summary>
        /// Called when window is enabled
        /// </summary>
        private void OnEnable()
        {
            // Create presenter and connect events
            _presenter = new ARMDebugPresenter();
            _presenter.OnStateChanged += Repaint;
            
            // Find index for current build target
            UpdateBuildTargetIndex();
        }
        
        /// <summary>
        /// Called when window is disabled
        /// </summary>
        private void OnDisable()
        {
            if (_presenter != null)
            {
                _presenter.OnStateChanged -= Repaint;
            }
        }
        
        /// <summary>
        /// Initialize UI styles
        /// </summary>
        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(0, 0, 10, 10)
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
            
            if (_toggleStyle == null)
            {
                _toggleStyle = new GUIStyle(EditorStyles.toggle)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }
            
            if (_descriptionStyle == null)
            {
                _descriptionStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    fontSize = 11,
                    margin = new RectOffset(20, 20, 10, 10)
                };
            }
            
            if (_symbolButtonStyle == null)
            {
                _symbolButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 22,
                    margin = new RectOffset(2, 2, 1, 1),
                    padding = new RectOffset(5, 5, 3, 3)
                };
            }
            
            if (_symbolBoxStyle == null)
            {
                _symbolBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(2, 2, 2, 2),
                    margin = new RectOffset(0, 0, 5, 5)
                };
            }
        }
        
        /// <summary>
        /// Update build target index
        /// </summary>
        private void UpdateBuildTargetIndex()
        {
            BuildTargetGroup currentGroup = _presenter.CurrentTargetGroup;
            
            for (int i = 0; i < _buildTargetGroups.Length; i++)
            {
                if (_buildTargetGroups[i] == currentGroup)
                {
                    _selectedBuildTargetIndex = i;
                    return;
                }
            }
            
            // Default value
            _selectedBuildTargetIndex = 0;
        }
        
        /// <summary>
        /// Draw GUI
        /// </summary>
        private void OnGUI()
        {
            InitializeStyles();
            
            EditorGUILayout.Space(10);
            
            // Header
            EditorGUILayout.LabelField("ARM Debug Settings", _headerStyle);
            
            EditorGUILayout.Space(10);
            
            // Main content
            EditorGUILayout.BeginVertical(_boxStyle);
            
            // Target platform selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Platform:", GUILayout.Width(100));
            int newTargetIndex = EditorGUILayout.Popup(_selectedBuildTargetIndex, _buildTargetNames);
            if (newTargetIndex != _selectedBuildTargetIndex)
            {
                _selectedBuildTargetIndex = newTargetIndex;
                _presenter.ChangeTargetGroup(_buildTargetGroups[newTargetIndex]);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(15);
            
            // Debugging mode toggle
            bool isEnabled = _presenter.IsDebuggingEnabled;
            bool newIsEnabled = EditorGUILayout.Toggle(
                new GUIContent("Enable ARM Debugging", "Adds ARM_DEBUGGING to Define Symbols"),
                isEnabled,
                _toggleStyle
            );
            
            if (newIsEnabled != isEnabled)
            {
                _presenter.SetDebuggingEnabled(newIsEnabled);
            }
            
            // Current state description
            string description = isEnabled
                ? "Debugging mode is enabled. 'ARM_DEBUGGING' Define Symbol has been applied."
                : "Debugging mode is disabled. 'ARM_DEBUGGING' Define Symbol has been removed.";
            
            EditorGUILayout.LabelField(description, _descriptionStyle);
            
            EditorGUILayout.Space(10);
            
            // Current symbols display with scrollable box
            EditorGUILayout.LabelField("Current Define Symbols:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(_symbolBoxStyle, GUILayout.Height(80));
            
            string[] symbols = _presenter.GetCurrentDefineSymbolsArray();
            
            _symbolsScrollPosition = EditorGUILayout.BeginScrollView(_symbolsScrollPosition);
            
            if (symbols.Length == 0)
            {
                EditorGUILayout.LabelField("No define symbols found", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = 0; i < symbols.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Set color for ARM_DEBUGGING symbol
                    if (symbols[i] == "ARM_DEBUGGING")
                    {
                        GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
                    }
                    
                    GUILayout.Button(symbols[i], _symbolButtonStyle);
                    
                    // Reset color
                    GUI.backgroundColor = Color.white;
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(15);
            
            // Buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh", GUILayout.Height(30)))
            {
                _presenter.RefreshState();
            }
            
            if (GUILayout.Button(isEnabled ? "Disable Debugging" : "Enable Debugging", GUILayout.Height(30)))
            {
                _presenter.ToggleDebuggingMode();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            // Bottom info
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This setting controls the debugging features of the ARM (Addressable Resource Manager) system. " +
                "When debugging mode is enabled, logging and tracking features are activated.",
                MessageType.Info
            );
        }
    }
}