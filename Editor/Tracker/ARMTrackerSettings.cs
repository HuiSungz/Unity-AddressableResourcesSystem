
using System;
using UnityEditor;
using UnityEngine;

namespace AddressableManage.Editor
{
    [Serializable]
    public class ARMTrackerSettings
    {
        // 정렬 옵션
        public static readonly string[] SortOptions = new[]
        {
            "Key",
            "Ref Count (High to Low)",
            "Ref Count (Low to High)",
            "Batch Loaded First",
            "Individual Loaded First",
            "Handle Count"
        };
        
        // 설정 키
        private const string PREF_PREFIX = "ARMTracker_";
        private const string PREF_REFRESH_INTERVAL = PREF_PREFIX + "RefreshInterval";
        private const string PREF_SORT_INDEX = PREF_PREFIX + "SortIndex";
        private const string PREF_SHOW_BATCH = PREF_PREFIX + "ShowBatch";
        private const string PREF_SHOW_INDIVIDUAL = PREF_PREFIX + "ShowIndividual";
        private const string PREF_WINDOW_X = PREF_PREFIX + "WindowX";
        private const string PREF_WINDOW_Y = PREF_PREFIX + "WindowY";
        private const string PREF_WINDOW_WIDTH = PREF_PREFIX + "WindowWidth";
        private const string PREF_WINDOW_HEIGHT = PREF_PREFIX + "WindowHeight";
        
        // 기본값
        private const float DEFAULT_REFRESH_INTERVAL = 1.0f;
        private const int DEFAULT_SORT_INDEX = 0;
        private const bool DEFAULT_SHOW_BATCH = true;
        private const bool DEFAULT_SHOW_INDIVIDUAL = true;
        
        // 설정 값
        public float RefreshInterval { get; set; } = DEFAULT_REFRESH_INTERVAL;
        public int SortIndex { get; set; } = DEFAULT_SORT_INDEX;
        public bool ShowBatchLoaded { get; set; } = DEFAULT_SHOW_BATCH;
        public bool ShowIndividualLoaded { get; set; } = DEFAULT_SHOW_INDIVIDUAL;
        public Rect WindowRect { get; set; } = new Rect(100, 100, 800, 600);
        
        // 설정 로드
        public static ARMTrackerSettings LoadSettings()
        {
            ARMTrackerSettings settings = new ARMTrackerSettings
            {
                RefreshInterval = EditorPrefs.GetFloat(PREF_REFRESH_INTERVAL, DEFAULT_REFRESH_INTERVAL),
                SortIndex = EditorPrefs.GetInt(PREF_SORT_INDEX, DEFAULT_SORT_INDEX),
                ShowBatchLoaded = EditorPrefs.GetBool(PREF_SHOW_BATCH, DEFAULT_SHOW_BATCH),
                ShowIndividualLoaded = EditorPrefs.GetBool(PREF_SHOW_INDIVIDUAL, DEFAULT_SHOW_INDIVIDUAL),
                WindowRect = new Rect(
                    EditorPrefs.GetFloat(PREF_WINDOW_X, 100),
                    EditorPrefs.GetFloat(PREF_WINDOW_Y, 100),
                    EditorPrefs.GetFloat(PREF_WINDOW_WIDTH, 800),
                    EditorPrefs.GetFloat(PREF_WINDOW_HEIGHT, 600)
                )
            };
            
            return settings;
        }
        
        // 설정 저장
        public void SaveSettings()
        {
            EditorPrefs.SetFloat(PREF_REFRESH_INTERVAL, RefreshInterval);
            EditorPrefs.SetInt(PREF_SORT_INDEX, SortIndex);
            EditorPrefs.SetBool(PREF_SHOW_BATCH, ShowBatchLoaded);
            EditorPrefs.SetBool(PREF_SHOW_INDIVIDUAL, ShowIndividualLoaded);
            EditorPrefs.SetFloat(PREF_WINDOW_X, WindowRect.x);
            EditorPrefs.SetFloat(PREF_WINDOW_Y, WindowRect.y);
            EditorPrefs.SetFloat(PREF_WINDOW_WIDTH, WindowRect.width);
            EditorPrefs.SetFloat(PREF_WINDOW_HEIGHT, WindowRect.height);
        }
        
        // 설정 리셋
        public void ResetToDefaults()
        {
            RefreshInterval = DEFAULT_REFRESH_INTERVAL;
            SortIndex = DEFAULT_SORT_INDEX;
            ShowBatchLoaded = DEFAULT_SHOW_BATCH;
            ShowIndividualLoaded = DEFAULT_SHOW_INDIVIDUAL;
            WindowRect = new Rect(100, 100, 800, 600);
            
            SaveSettings();
        }
    }
}