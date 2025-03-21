
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableManage.Editor
{
    public static class ARMTrackerUtils
    {
        // 비동기 핸들에서 추가 정보 추출
        public static string GetHandleInfo(this AsyncOperationHandle handle)
        {
            try
            {
                string status = handle.Status.ToString();
                float progress = handle.PercentComplete;
                Type resultType = handle.Result?.GetType();
                string resultTypeName = resultType != null ? resultType.Name : "null";
                
                return $"{status} ({progress:P0}) - {resultTypeName}";
            }
            catch (Exception)
            {
                return "Error reading handle info";
            }
        }
        
        // 확장 메서드: 유니티 오브젝트가 에셋을 참조하는지 확인
        public static bool ReferencesAsset(this UnityEngine.Object obj, AssetEntry entry)
        {
            if (obj == null) return false;
            
            try
            {
                // 오브젝트의 필드를 검사해 에셋 참조 여부 확인
                Type type = obj.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(UnityEngine.Object) || 
                        (field.FieldType.IsClass && typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)))
                    {
                        var value = field.GetValue(obj) as UnityEngine.Object;
                        if (value != null)
                        {
                            string assetPath = AssetDatabase.GetAssetPath(value);
                            if (!string.IsNullOrEmpty(assetPath) && assetPath.Contains(entry.Key))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // 에러 무시
            }
            
            return false;
        }
        
        // 에디터 UI용: 진행 상태 막대 그리기
        public static void DrawProgressBar(Rect rect, float progress, Color color, string tooltip = null)
        {
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
            
            Rect progressRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(progress), rect.height);
            EditorGUI.DrawRect(progressRect, color);
            
            if (!string.IsNullOrEmpty(tooltip) && rect.Contains(Event.current.mousePosition))
            {
                GUIStyle tooltipStyle = new GUIStyle
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    fontStyle = FontStyle.Bold
                };
                
                GUI.Label(rect, tooltip, tooltipStyle);
            }
        }
        
        // 핸들 상태에 따른 색상 가져오기
        public static Color GetStatusColor(AsyncOperationStatus status)
        {
            switch (status)
            {
                case AsyncOperationStatus.Succeeded:
                    return new Color(0.0f, 0.8f, 0.2f);
                case AsyncOperationStatus.Failed:
                    return new Color(0.8f, 0.0f, 0.0f);
                default:
                    return new Color(0.8f, 0.8f, 0.0f);
            }
        }
    }
}