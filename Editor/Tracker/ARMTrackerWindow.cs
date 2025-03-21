
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AddressableManage.Editor
{
    public class ARMTrackerWindow : EditorWindow
    {
        // 싱글톤 인스턴스
        private static ARMTrackerWindow _instance;
        
        // 설정 및 데이터 관리자
        private ARMTrackerSettings _settings;
        private ARMTrackerReflectionData _data;
        
        // UI 상태 변수
        private Vector2 _assetListScrollPosition;
        private Vector2 _detailScrollPosition;
        private float _splitViewPosition = 500f;
        private AssetEntryProxy _selectedEntry;
        private string _searchText = "";
        
        // UI 설정
        private readonly Color _evenRowColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);
        private readonly Color _oddRowColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        private readonly Color _selectedRowColor = new Color(0.3f, 0.7f, 0.9f, 0.4f);
        private readonly Color _batchLoadedColor = new Color(1f, 0.6f, 0.2f, 0.8f);
        private readonly Color _individualLoadedColor = new Color(0.2f, 0.9f, 0.4f, 0.8f);
        private GUIStyle _headerStyle;
        private GUIStyle _boldLabelStyle;
        private GUIStyle _assetRowStyle;
        private GUIStyle _centeredLabelStyle;
        
        // 리프레시 타이머
        private double _lastRefreshTime;
        
        [MenuItem("ARM/Reference Tracker")]
        public static void ShowWindow()
        {
            _instance = GetWindow<ARMTrackerWindow>("ARM Tracker");
            _instance.minSize = new Vector2(1200, 600);
        }
        
        private void OnEnable()
        {
            _settings = ARMTrackerSettings.LoadSettings();
            _data = new ARMTrackerReflectionData();
            
            // 스타일 초기화는 OnGUI에서 수행
            
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // 플레이 모드 진입 시 데이터 리셋
                _data.Reset();
            }
        }
        
        private void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying)
                return;
                
            // 설정된 간격으로 데이터 업데이트
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - _lastRefreshTime >= _settings.RefreshInterval)
            {
                _lastRefreshTime = currentTime;
                _data.Refresh();
                Repaint();
            }
        }
        
        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 12
                };
            }
            
            if (_boldLabelStyle == null)
            {
                _boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            }
            
            if (_assetRowStyle == null)
            {
                _assetRowStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(5, 5, 3, 3),
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }
            
            if (_centeredLabelStyle == null)
            {
                _centeredLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            // 플레이 모드가 아닐 때 상단에 알림 배너 표시
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
            }
            
            DrawToolbar();
            
            EditorGUILayout.BeginHorizontal();
            
            // 왼쪽 패널 - 에셋 리스트
            DrawAssetList();
            
            // 리사이즈 핸들
            _splitViewPosition = ResizeSplitView(_splitViewPosition, 300, position.width - 300);
            
            // 오른쪽 패널 - 선택된 에셋 상세 정보
            DrawDetailPanel();
            
            EditorGUILayout.EndHorizontal();
            
            DrawStatusBar();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _data.Refresh();
            }
            
            EditorGUILayout.Space();
            
            // 검색 필드
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
            
            // 정렬 옵션
            EditorGUILayout.LabelField("Sort by:", GUILayout.Width(50));
            int newSortIndex = EditorGUILayout.Popup(_settings.SortIndex, ARMTrackerSettings.SortOptions, EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (newSortIndex != _settings.SortIndex)
            {
                _settings.SortIndex = newSortIndex;
                _settings.SaveSettings();
                _data.ApplySorting(_settings.SortIndex);
            }
            
            EditorGUILayout.Space();
            
            // 표시 필터
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
            
            // 업데이트 간격 설정
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
            
            // 헤더
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Asset Key", _headerStyle, GUILayout.Width(_splitViewPosition * 0.5f));
            GUILayout.Label("Ref Count", _headerStyle, GUILayout.Width(_splitViewPosition * 0.15f));
            GUILayout.Label("Is Batch", _headerStyle, GUILayout.Width(_splitViewPosition * 0.15f));
            GUILayout.Label("Handles", _headerStyle, GUILayout.Width(_splitViewPosition * 0.2f));
            EditorGUILayout.EndHorizontal();
            
            // 리스트 영역
            _assetListScrollPosition = EditorGUILayout.BeginScrollView(_assetListScrollPosition);
            
            // 데이터 리스트 그리기
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
            
            // 행 배경색 설정
            Color backgroundColor = isSelected ? _selectedRowColor : (index % 2 == 0 ? _evenRowColor : _oddRowColor);
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            
            EditorGUILayout.BeginHorizontal(_assetRowStyle);
            
            // 배치/개별 로드 표시 색상 마커
            Rect colorRect = GUILayoutUtility.GetRect(4, 20, GUILayout.Width(4));
            EditorGUI.DrawRect(colorRect, entry.IsBatchLoaded ? _batchLoadedColor : _individualLoadedColor);
            
            // 키 이름
            if (GUILayout.Button(entry.Key, EditorStyles.label, GUILayout.Width(_splitViewPosition * 0.5f - 4)))
            {
                _selectedEntry = entry;
            }
            
            // 참조 카운트
            GUILayout.Label(entry.ReferenceCount.ToString(), GUILayout.Width(_splitViewPosition * 0.15f));
            
            // 배치 로드 여부
            GUILayout.Label(entry.IsBatchLoaded ? "Yes" : "No", GUILayout.Width(_splitViewPosition * 0.15f));
            
            // 핸들 개수
            GUILayout.Label(entry.HandleMap.Count.ToString(), GUILayout.Width(_splitViewPosition * 0.2f));
            
            EditorGUILayout.EndHorizontal();
            
            GUI.backgroundColor = oldColor;
        }
        
        private void DrawDetailPanel()
        {
            EditorGUILayout.BeginVertical();
            
            // 헤더
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
                
                // 에셋 정보 헤더
                GUILayout.Label("Asset Information", _boldLabelStyle);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Key:", _selectedEntry.Key);
                EditorGUILayout.LabelField("Batch Loaded:", _selectedEntry.IsBatchLoaded.ToString());
                EditorGUILayout.LabelField("Reference Count:", _selectedEntry.ReferenceCount.ToString());
                EditorGUILayout.LabelField("Handle Count:", _selectedEntry.HandleMap.Count.ToString());
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(10);
                
                // 핸들 목록
                GUILayout.Label("Handle Map", _boldLabelStyle);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // 핸들 테이블 헤더
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("Resource Location", _headerStyle, GUILayout.Width(250));
                GUILayout.Label("Status", _headerStyle, GUILayout.Width(100));
                GUILayout.Label("Type", _headerStyle, GUILayout.Width(150));
                GUILayout.Label("Percent Complete", _headerStyle, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                // 핸들 항목
                int index = 0;
                foreach (var handlePair in _selectedEntry.HandleMap)
                {
                    DrawHandleRow(handlePair.Key, handlePair.Value, index++);
                }
                
                EditorGUILayout.EndVertical();
                
                // 참조 추적 (가능한 경우)
                EditorGUILayout.Space(10);
                GUILayout.Label("Reference Usage", _boldLabelStyle);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Note: This feature is limited at runtime", EditorStyles.miniLabel);
                
                // 참조 추적
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
        
        private void DrawHandleRow(IResourceLocation location, AsyncOperationHandle handle, int index)
        {
            Color backgroundColor = index % 2 == 0 ? _evenRowColor : _oddRowColor;
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            
            EditorGUILayout.BeginHorizontal(_assetRowStyle);
            
            GUILayout.Label(location.PrimaryKey, GUILayout.Width(250));
            GUILayout.Label(handle.Status.ToString(), GUILayout.Width(100));
            GUILayout.Label(location.ResourceType?.Name ?? "Unknown", GUILayout.Width(150));
            GUILayout.Label($"{handle.PercentComplete * 100:F1}%", GUILayout.Width(100));
            
            EditorGUILayout.EndHorizontal();
            
            GUI.backgroundColor = oldColor;
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
                GUILayout.Label($"Last Updated: {DateTime.Now.ToString("HH:mm:ss")}");
            }
            else
            {
                GUILayout.Label("Enter play mode to start tracking");
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private float ResizeSplitView(float currentPosition, float minSize, float maxSize)
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
                currentPosition = Mathf.Clamp(Event.current.mousePosition.x, minSize, maxSize);
                Event.current.Use();
                Repaint();
            }
            
            if (_resizing && Event.current.type == EventType.MouseUp)
            {
                Event.current.Use();
                _resizing = false;
            }
            
            // 리사이즈 핸들 시각화
            Color oldColor = GUI.color;
            GUI.color = Color.grey;
            GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
            GUI.color = oldColor;
            
            return currentPosition;
        }
        
        private bool _resizing = false;
    }
}