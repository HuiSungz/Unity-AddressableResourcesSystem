
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ArchitectHS.AddressableManage.Editor
{
    public class ARMTrackerWindow : EditorWindow
    {
        // Singleton instance
        private static ARMTrackerWindow _instance;

        private ARMTrackerSettings _settings;
        private ARMTrackerReflectionData _data;

        private Vector2 _assetListScrollPosition;
        private Vector2 _detailScrollPosition;
        private float _splitViewPosition = 500f;
        private AssetEntryProxy _selectedEntry;
        private string _searchText = "";
        private bool _initialized;
        
        // UI settings
        private readonly Color _evenRowColor = new(0.8f, 0.8f, 0.8f, 0.1f);
        private readonly Color _oddRowColor = new(0.8f, 0.8f, 0.8f, 0.2f);
        private readonly Color _selectedRowColor = new(0.3f, 0.7f, 0.9f, 0.4f);
        private readonly Color _batchLoadedColor = new(1f, 0.6f, 0.2f, 0.8f);
        private readonly Color _individualLoadedColor = new(0.2f, 0.9f, 0.4f, 0.8f);
        private GUIStyle _headerStyle;
        private GUIStyle _boldLabelStyle;
        private GUIStyle _assetRowStyle;
        private GUIStyle _centeredLabelStyle;

        private double _lastRefreshTime;
        private bool _resizing;
        
        [MenuItem("ArchitectHS/ARM/Reference Tracker")]
        public static void ShowWindow()
        {
            _instance = GetWindow<ARMTrackerWindow>("ARM Tracker");
            _instance.minSize = new Vector2(1200, 600);
        }
        
        private void OnEnable()
        {
            InitializeIfNeeded();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.update -= OnEditorUpdate;
            CleanupData();
        }
        
        private void InitializeIfNeeded()
        {
            if (_initialized)
            {
                return;
            }
            
            _settings = ARMTrackerSettings.LoadSettings();
            _data = new ARMTrackerReflectionData();
            _initialized = true;
        }

        private void CleanupData()
        {
            if (_data != null)
            {
                _data.Reset();
                _data = null;
            }
            _selectedEntry = null;
            _initialized = false;
        }
        
        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    InitializeIfNeeded();
                    if (_data != null)
                    {
                        _data.Reset();
                        _data.InitializeReflectionInfo(); // Reinitialize reflection data
                    }
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    _data?.Reset();
                    _selectedEntry = null;
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    CleanupData();
                    Repaint();
                    break;
            }
        }
        
        private void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying || _data == null)
                return;
                
            var currentTime = EditorApplication.timeSinceStartup;
            if (!(currentTime - _lastRefreshTime >= _settings.RefreshInterval))
            {
                return;
            }
            
            _lastRefreshTime = currentTime;
            
            try
            {
                _data.Refresh();
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ARM Tracker] Error during refresh: {ex.Message}");
            }
        }
        
        private void InitializeStyles()
        {
            _headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12
            };
            
            _boldLabelStyle ??= new GUIStyle(EditorStyles.boldLabel);
            
            _assetRowStyle ??= new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(5, 5, 3, 3),
                margin = new RectOffset(0, 0, 0, 0)
            };
            
            _centeredLabelStyle ??= new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }
        
        private void OnGUI()
        {
            InitializeIfNeeded();
            InitializeStyles();
            
            // Display notification banner when not in play mode
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("ARM Tracker is only available in Play Mode. Press Play to start tracking assets.", 
                    EditorStyles.boldLabel);
                if (GUILayout.Button("Enter Play Mode", GUILayout.Width(120)))
                {
                    EditorApplication.isPlaying = true;
                }
                EditorGUILayout.EndHorizontal();
                return; // Don't draw the rest of the UI when not in play mode
            }

            if (_data == null)
            {
                EditorGUILayout.HelpBox("Tracker data is not initialized. Try re-entering play mode.", MessageType.Warning);
                return;
            }
            
            try
            {
                DrawToolbar();
                
                EditorGUILayout.BeginHorizontal();
                DrawAssetList();
                _splitViewPosition = ResizeSplitView(_splitViewPosition, 300, position.width - 300);
                DrawDetailPanel();
                EditorGUILayout.EndHorizontal();
                
                DrawStatusBar();
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox($"Error drawing UI: {ex.Message}", MessageType.Error);
                Debug.LogError($"[ARM Tracker] GUI Error: {ex}");
            }
        }

        private void DrawHandleRow(IResourceLocation location, AsyncOperationHandle handle, int index)
        {
            if (!handle.IsValid())
            {
                return; // Skip invalid handles
            }

            Color backgroundColor = index % 2 == 0 ? _evenRowColor : _oddRowColor;
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            
            EditorGUILayout.BeginHorizontal(_assetRowStyle);
            
            try
            {
                GUILayout.Label(location.PrimaryKey, GUILayout.Width(250));
                GUILayout.Label(handle.Status.ToString(), GUILayout.Width(100));
                GUILayout.Label(location.ResourceType?.Name ?? "Unknown", GUILayout.Width(150));
                GUILayout.Label($"{handle.PercentComplete * 100:F1}%", GUILayout.Width(100));
            }
            catch (Exception)
            {
                GUILayout.Label("Invalid Handle", GUILayout.Width(250));
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUI.backgroundColor = oldColor;
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _data.Refresh();
            }
            
            EditorGUILayout.Space();
            
            // Search field
            EditorGUILayout.BeginHorizontal();
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(18)))
            {
                _searchText = "";
                GUI.FocusControl(null);
            }
            if (GUILayout.Button("Search", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                _data.ApplyFilter(_searchText);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Sorting options
            EditorGUILayout.LabelField("Sort by:", GUILayout.Width(50));
            int newSortIndex = EditorGUILayout.Popup(_settings.SortIndex, ARMTrackerSettings.SortOptions, EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (newSortIndex != _settings.SortIndex)
            {
                _settings.SortIndex = newSortIndex;
                _settings.SaveSettings();
                _data.ApplySorting(_settings.SortIndex);
            }
            
            EditorGUILayout.Space();
            
            // Visibility filters
            EditorGUILayout.LabelField("Show:", GUILayout.Width(40));
            
            bool newShowBatch = GUILayout.Toggle(_settings.ShowBatchLoaded, "Batch", EditorStyles.toolbarButton, GUILayout.Width(50));
            bool newShowIndividual = GUILayout.Toggle(_settings.ShowIndividualLoaded, "Individual", EditorStyles.toolbarButton, GUILayout.Width(70));
            
            if (newShowBatch != _settings.ShowBatchLoaded || newShowIndividual != _settings.ShowIndividualLoaded)
            {
                _settings.ShowBatchLoaded = newShowBatch;
                _settings.ShowIndividualLoaded = newShowIndividual;
                _settings.SaveSettings();
                _data.ApplyVisibilityFilters(_settings.ShowBatchLoaded, _settings.ShowIndividualLoaded);
            }
            
            EditorGUILayout.Space(20);
            
            // Refresh rate settings
            EditorGUILayout.LabelField("Refresh Rate:", GUILayout.Width(80));
            float newInterval = EditorGUILayout.Slider(_settings.RefreshInterval, 0.1f, 5.0f, GUILayout.Width(150));
            if (Math.Abs(newInterval - _settings.RefreshInterval) > 0.01f)
            {
                _settings.RefreshInterval = newInterval;
                _settings.SaveSettings();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawAssetList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_splitViewPosition));
            
            // Header
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Asset Key", _headerStyle, GUILayout.Width(_splitViewPosition * 0.5f));
            GUILayout.Label("Ref Count", _headerStyle, GUILayout.Width(_splitViewPosition * 0.15f));
            GUILayout.Label("Is Batch", _headerStyle, GUILayout.Width(_splitViewPosition * 0.15f));
            GUILayout.Label("Handles", _headerStyle, GUILayout.Width(_splitViewPosition * 0.2f));
            EditorGUILayout.EndHorizontal();
            
            // List area
            _assetListScrollPosition = EditorGUILayout.BeginScrollView(_assetListScrollPosition);
            
            // Draw data list
            List<AssetEntryProxy> entries = _data.GetVisibleEntries();
            if (entries.Count == 0)
            {
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.LabelField("No assets loaded or tracked.", _centeredLabelStyle);
                }
                else
                {
                    EditorGUILayout.LabelField("Enter play mode to track assets.", _centeredLabelStyle);
                }
            }
            else
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    DrawAssetRow(entries[i], i);
                }
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAssetRow(AssetEntryProxy entry, int index)
        {
            bool isSelected = _selectedEntry == entry;
            
            // Set row background color
            Color backgroundColor = isSelected ? _selectedRowColor : (index % 2 == 0 ? _evenRowColor : _oddRowColor);
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            
            EditorGUILayout.BeginHorizontal(_assetRowStyle);
            
            // Batch/Individual load color marker
            Rect colorRect = GUILayoutUtility.GetRect(4, 20, GUILayout.Width(4));
            EditorGUI.DrawRect(colorRect, entry.IsBatchLoaded ? _batchLoadedColor : _individualLoadedColor);
            
            // Key name
            if (GUILayout.Button(entry.Key, EditorStyles.label, GUILayout.Width(_splitViewPosition * 0.5f - 4)))
            {
                _selectedEntry = entry;
            }
            
            // Reference Count
            GUILayout.Label(entry.ReferenceCount.ToString(), GUILayout.Width(_splitViewPosition * 0.15f));
            
            // Batch load status
            GUILayout.Label(entry.IsBatchLoaded ? "Yes" : "No", GUILayout.Width(_splitViewPosition * 0.15f));
            
            // Handle Count
            GUILayout.Label(entry.HandleMap.Count.ToString(), GUILayout.Width(_splitViewPosition * 0.2f));
            
            EditorGUILayout.EndHorizontal();
            
            GUI.backgroundColor = oldColor;
        }
        
        private void DrawDetailPanel()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Detail View", _headerStyle);
            EditorGUILayout.EndHorizontal();
            
            _detailScrollPosition = EditorGUILayout.BeginScrollView(_detailScrollPosition);
            
            if (_selectedEntry == null)
            {
                EditorGUILayout.LabelField("Select an asset to view details", _centeredLabelStyle);
            }
            else
            {
                EditorGUILayout.Space(10);
                
                // Asset Information header
                GUILayout.Label("Asset Information", _boldLabelStyle);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Key:", _selectedEntry.Key);
                EditorGUILayout.LabelField("Batch Loaded:", _selectedEntry.IsBatchLoaded.ToString());
                EditorGUILayout.LabelField("Reference Count:", _selectedEntry.ReferenceCount.ToString());
                EditorGUILayout.LabelField("Handle Count:", _selectedEntry.HandleMap.Count.ToString());
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(10);
                
                // Handle Map
                GUILayout.Label("Handle Map", _boldLabelStyle);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Handle table header
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("Resource Location", _headerStyle, GUILayout.Width(250));
                GUILayout.Label("Status", _headerStyle, GUILayout.Width(100));
                GUILayout.Label("Type", _headerStyle, GUILayout.Width(150));
                GUILayout.Label("Percent Complete", _headerStyle, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                // Handle row
                int index = 0;
                foreach (var handlePair in _selectedEntry.HandleMap)
                {
                    DrawHandleRow(handlePair.Key, handlePair.Value, index++);
                }
                
                EditorGUILayout.EndVertical();
                
                // Reference tracking (if available)
                EditorGUILayout.Space(10);
                GUILayout.Label("Reference Usage", _boldLabelStyle);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Note: This feature is limited at runtime", EditorStyles.miniLabel);
                
                // Reference tracking
                var usages = _data.FindReferences(_selectedEntry);
                if (usages.Count == 0)
                {
                    EditorGUILayout.LabelField("No usage information available", _centeredLabelStyle);
                }
                else
                {
                    foreach (var usage in usages)
                    {
                        EditorGUILayout.LabelField(usage);
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label($"Total Assets: {_data.TotalAssetCount}");
            GUILayout.Label($"Batch Loaded: {_data.BatchLoadedCount}");
            GUILayout.Label($"Individual Loaded: {_data.IndividualLoadedCount}");
            
            GUILayout.FlexibleSpace();
            
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label($"Last Updated: {DateTime.Now:HH:mm:ss}");
            }
            else
            {
                GUILayout.Label("Enter play mode to start tracking");
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private float ResizeSplitView(float currentPosition, float paramMinSize, float paramMaxSize)
        {
            Rect resizeRect = new Rect(currentPosition, 0, 5f, position.height);
            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
            
            if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                _resizing = true;
            }
            
            if (_resizing && Event.current.type == EventType.MouseDrag)
            {
                currentPosition = Mathf.Clamp(Event.current.mousePosition.x, paramMinSize, paramMaxSize);
                Event.current.Use();
                Repaint();
            }
            
            if (_resizing && Event.current.type == EventType.MouseUp)
            {
                Event.current.Use();
                _resizing = false;
            }
            
            var oldColor = GUI.color;
            GUI.color = Color.grey;
            GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
            GUI.color = oldColor;
            
            return currentPosition;
        }
    }
}