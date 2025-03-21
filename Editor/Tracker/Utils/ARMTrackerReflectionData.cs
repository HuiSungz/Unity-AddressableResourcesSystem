
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

namespace AddressableManage.Editor
{
    /// <summary>
    /// 에셋 엔트리를 위한 래퍼 클래스로, 리플렉션을 통해 내부 데이터에 접근합니다.
    /// </summary>
    public class AssetEntryProxy
    {
        private readonly object _originalEntry;
        private readonly Type _entryType;

        public string Key { get; private set; } = "Unknown";
        public bool IsBatchLoaded { get; private set; }
        public ushort ReferenceCount { get; private set; }
        public Dictionary<IResourceLocation, AsyncOperationHandle> HandleMap { get; private set; } 
            = new Dictionary<IResourceLocation, AsyncOperationHandle>();

        public AssetEntryProxy(object originalEntry)
        {
            _originalEntry = originalEntry ?? throw new ArgumentNullException(nameof(originalEntry));
            _entryType = originalEntry.GetType();
            
            try
            {
                // Key 추출 - 필드 먼저 시도, 없으면 프로퍼티 시도
                var keyField = _entryType.GetField("Key", BindingFlags.Public | BindingFlags.Instance);
                if (keyField != null)
                {
                    var keyValue = keyField.GetValue(originalEntry);
                    if (keyValue != null)
                        Key = keyValue.ToString();
                }
                else
                {
                    var keyProperty = _entryType.GetProperty("Key");
                    if (keyProperty != null)
                    {
                        var keyValue = keyProperty.GetValue(originalEntry);
                        if (keyValue != null)
                            Key = keyValue.ToString();
                    }
                }
                
                // IsBatchLoaded 추출 - 필드 먼저 시도, 없으면 프로퍼티 시도
                var batchField = _entryType.GetField("IsBatchLoaded", BindingFlags.Public | BindingFlags.Instance);
                if (batchField != null)
                {
                    var batchValue = batchField.GetValue(originalEntry);
                    if (batchValue != null)
                        IsBatchLoaded = (bool)batchValue;
                }
                else
                {
                    var batchProperty = _entryType.GetProperty("IsBatchLoaded");
                    if (batchProperty != null)
                    {
                        var batchValue = batchProperty.GetValue(originalEntry);
                        if (batchValue != null)
                            IsBatchLoaded = (bool)batchValue;
                    }
                }
                
                // 필드 추출
                FieldInfo refCountField = _entryType.GetField("ReferenceCount", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (refCountField != null)
                {
                    var refCountValue = refCountField.GetValue(originalEntry);
                    if (refCountValue != null)
                        ReferenceCount = (ushort)refCountValue;
                }

                FieldInfo handleMapField = _entryType.GetField("HandleMap", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (handleMapField != null)
                {
                    var handleMapValue = handleMapField.GetValue(originalEntry);
                    if (handleMapValue != null)
                    {
                        HandleMap = (Dictionary<IResourceLocation, AsyncOperationHandle>)handleMapValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ARM Tracker] 에셋 엔트리 속성 추출 중 오류: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public object GetOriginalEntry()
        {
            return _originalEntry;
        }

        public void RefreshData()
        {
            try
            {
                // 필드 추출
                FieldInfo refCountField = _entryType.GetField("ReferenceCount", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (refCountField != null)
                {
                    var refCountValue = refCountField.GetValue(_originalEntry);
                    if (refCountValue != null)
                        ReferenceCount = (ushort)refCountValue;
                }

                FieldInfo handleMapField = _entryType.GetField("HandleMap", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (handleMapField != null)
                {
                    var handleMapValue = handleMapField.GetValue(_originalEntry);
                    if (handleMapValue != null)
                    {
                        HandleMap = (Dictionary<IResourceLocation, AsyncOperationHandle>)handleMapValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ARM Tracker] 에셋 엔트리 데이터 새로고침 중 오류: {ex.Message}");
            }
        }
    }

    public class ARMTrackerReflectionData
    {
        // 기존 데이터
        private readonly List<AssetEntryProxy> _allEntries = new List<AssetEntryProxy>();
        private readonly List<AssetEntryProxy> _visibleEntries = new List<AssetEntryProxy>();
        private readonly Dictionary<AssetEntryProxy, List<string>> _referenceCache = new Dictionary<AssetEntryProxy, List<string>>();
        
        // 필터 상태
        private string _currentFilter = "";
        private bool _showBatchLoaded = true;
        private bool _showIndividualLoaded = true;
        private int _sortIndex = 0;
        
        // 통계 정보
        public int TotalAssetCount => _allEntries.Count;
        public int BatchLoadedCount => _allEntries.Count(e => e.IsBatchLoaded);
        public int IndividualLoadedCount => _allEntries.Count(e => !e.IsBatchLoaded);
        
        // 캐시된 리플렉션 정보
        private Type _armType;
        private Type _registryType;
        private PropertyInfo _instanceProperty;
        private FieldInfo _registryField;
        private PropertyInfo _assetsProperty;
        private FieldInfo _instanceField;
        private FieldInfo _assetsField;
        
        public ARMTrackerReflectionData()
        {
            InitializeReflectionInfo();
            Reset();
        }
        
        private void InitializeReflectionInfo()
        {
            try
            {
                // 모든 어셈블리에서 ARM 클래스 찾기
                _armType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // 에디터 어셈블리는 건너뛰기
                    if (assembly.GetName().Name.Contains("Editor"))
                        continue;
                        
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => t.Namespace == "AddressableManage" && t.Name == "ARM")
                            .ToArray();
                            
                        if (types.Length > 0)
                        {
                            _armType = types[0];
                            Debug.Log($"[ARM Tracker] ARM 타입을 찾았습니다: {_armType.FullName}, 어셈블리: {assembly.GetName().Name}");
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // 일부 어셈블리는 GetTypes()가 실패할 수 있음 - 무시하고 계속 진행
                        continue;
                    }
                }
                
                if (_armType == null)
                {
                    // Fallback: 직접 어셈블리 이름으로 시도
                    try 
                    {
                        var mainAssembly = Assembly.Load("Assembly-CSharp");
                        _armType = mainAssembly.GetType("AddressableManage.ARM");
                        Debug.Log($"[ARM Tracker] Fallback으로 ARM 타입을 찾았습니다: {_armType?.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ARM Tracker] ARM 타입을 찾을 수 없습니다. 오류: {ex.Message}");
                        return;
                    }
                }
                
                if (_armType == null)
                {
                    Debug.LogError("[ARM Tracker] ARM 타입을 찾을 수 없습니다.");
                    return;
                }
                
                // Instance 프로퍼티 찾기 (Public 또는 NonPublic 모두 시도)
                _instanceProperty = _armType.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    
                if (_instanceProperty == null)
                {
                    // Fallback: 다른 싱글톤 패턴 일 수 있으므로 다른 이름의 정적 필드나 속성 찾기
                    var staticProperties = _armType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    var staticFields = _armType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    
                    // 우선 타입과 일치하는 static 속성이나 필드 찾기
                    foreach (var prop in staticProperties)
                    {
                        if (prop.PropertyType == _armType)
                        {
                            _instanceProperty = prop;
                            Debug.Log($"[ARM Tracker] 대체 싱글톤 속성을 찾았습니다: {prop.Name}");
                            break;
                        }
                    }
                    
                    if (_instanceProperty == null)
                    {
                        // 정적 필드 확인
                        foreach (var field in staticFields)
                        {
                            if (field.FieldType == _armType)
                            {
                                // 필드를 사용하기 위한 래퍼 속성 생성
                                _instanceField = field;
                                Debug.Log($"[ARM Tracker] 대체 싱글톤 필드를 찾았습니다: {field.Name}");
                                break;
                            }
                        }
                    }
                }
                
                if (_instanceProperty == null && _instanceField == null)
                {
                    Debug.LogError("[ARM Tracker] ARM의 싱글톤 인스턴스를 찾을 수 없습니다.");
                    return;
                }
                
                // _registry 필드 찾기
                _registryField = _armType.GetField("_registry", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_registryField == null)
                {
                    // Fallback: 다른 이름의 필드 찾기
                    var candidateFields = _armType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => f.Name.ToLower().Contains("registry") || f.Name.ToLower().Contains("asset"))
                        .ToArray();
                        
                    if (candidateFields.Length > 0)
                    {
                        _registryField = candidateFields[0];
                        Debug.Log($"[ARM Tracker] 대체 레지스트리 필드를 찾았습니다: {_registryField.Name}");
                    }
                }
                
                if (_registryField == null)
                {
                    Debug.LogError("[ARM Tracker] ARM의 _registry 필드를 찾을 수 없습니다.");
                    return;
                }
                
                // 레지스트리 타입 가져오기
                _registryType = _registryField.FieldType;
                Debug.Log($"[ARM Tracker] 레지스트리 타입: {_registryType.FullName}");
                
                // Assets 프로퍼티 찾기
                _assetsProperty = _registryType.GetProperty("Assets");
                if (_assetsProperty == null)
                {
                    // Fallback: 다른 이름의 프로퍼티나 필드 찾기
                    var candidateProps = _registryType.GetProperties()
                        .Where(p => p.Name.ToLower().Contains("asset") && 
                               p.PropertyType.IsGenericType && 
                               p.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        .ToArray();
                        
                    if (candidateProps.Length > 0)
                    {
                        _assetsProperty = candidateProps[0];
                        Debug.Log($"[ARM Tracker] 대체 Assets 프로퍼티를 찾았습니다: {_assetsProperty.Name}");
                    }
                    else
                    {
                        // 필드도 확인
                        var candidateFields = _registryType.GetFields()
                            .Where(f => f.Name.ToLower().Contains("asset") && 
                                   f.FieldType.IsGenericType && 
                                   f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                            .ToArray();
                            
                        if (candidateFields.Length > 0)
                        {
                            _assetsField = candidateFields[0];
                            Debug.Log($"[ARM Tracker] 대체 Assets 필드를 찾았습니다: {_assetsField.Name}");
                        }
                    }
                }
                
                if (_assetsProperty == null && _assetsField == null)
                {
                    Debug.LogError("[ARM Tracker] Assets 컬렉션을 찾을 수 없습니다.");
                    return;
                }
                
                Debug.Log("[ARM Tracker] 리플렉션 초기화를 성공적으로 완료했습니다!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ARM Tracker] 리플렉션 초기화 중 오류: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public void Reset()
        {
            _allEntries.Clear();
            _visibleEntries.Clear();
            _referenceCache.Clear();
        }
        
        public List<AssetEntryProxy> GetVisibleEntries()
        {
            return _visibleEntries;
        }
        
        public void Refresh()
        {
            _allEntries.Clear();
            _referenceCache.Clear();
            
            // 플레이 모드 체크
            if (!EditorApplication.isPlaying)
            {
                Debug.Log("[ARM Tracker] 플레이 모드에서만 데이터를 조회할 수 있습니다.");
                return;
            }
            
            try
            {
                // ARM 초기화 확인
                bool armInitialized = false;
                try
                {
                    PropertyInfo initializedProperty = _armType.GetProperty("Initialized", BindingFlags.Public | BindingFlags.Static);
                    if (initializedProperty != null)
                    {
                        armInitialized = (bool)initializedProperty.GetValue(null);
                    }
                }
                catch
                {
                    // Initialized 프로퍼티가 없을 수 있음 - 계속 진행
                }
                
                if (!armInitialized)
                {
                    Debug.LogWarning("[ARM Tracker] ARM이 초기화되지 않았거나 확인할 수 없습니다. 계속 진행합니다.");
                }
                
                // ARM 인스턴스 가져오기
                object armInstance = null;
                
                if (_instanceProperty != null)
                {
                    armInstance = _instanceProperty.GetValue(null);
                }
                else if (_instanceField != null)
                {
                    armInstance = _instanceField.GetValue(null);
                }
                
                if (armInstance == null)
                {
                    Debug.LogWarning("[ARM Tracker] ARM 인스턴스를 찾을 수 없습니다. ARM이 초기화되었는지 확인하세요.");
                    return;
                }
                
                // AssetRegistry 인스턴스 가져오기
                object registryInstance = _registryField.GetValue(armInstance);
                if (registryInstance == null)
                {
                    Debug.LogError("[ARM Tracker] AssetRegistry 인스턴스를 찾을 수 없습니다.");
                    return;
                }
                
                // Assets 딕셔너리 가져오기
                object assetsDictionary;
                if (_assetsProperty != null)
                {
                    assetsDictionary = _assetsProperty.GetValue(registryInstance);
                }
                else if (_assetsField != null)
                {
                    assetsDictionary = _assetsField.GetValue(registryInstance);
                }
                else
                {
                    Debug.LogError("[ARM Tracker] Assets 컬렉션 접근 방법을 찾을 수 없습니다.");
                    return;
                }
                
                if (assetsDictionary == null)
                {
                    Debug.LogWarning("[ARM Tracker] Assets 딕셔너리가 null입니다.");
                    return;
                }
                
                // 딕셔너리의 Values 프로퍼티 가져오기
                try
                {
                    var dictionaryType = assetsDictionary.GetType();
                    var valuesProperty = dictionaryType.GetProperty("Values");
                    
                    if (valuesProperty == null)
                    {
                        Debug.LogError("[ARM Tracker] 딕셔너리 Values 프로퍼티를 찾을 수 없습니다.");
                        return;
                    }
                    
                    var entriesCollection = valuesProperty.GetValue(assetsDictionary);
                    
                    if (entriesCollection == null)
                    {
                        Debug.LogWarning("[ARM Tracker] 에셋 엔트리 컬렉션이 null입니다.");
                        return;
                    }
                    
                    // 컬렉션 순회 방법 확인 (IEnumerable 구현체여야 함)
                    var enumerableType = entriesCollection.GetType();
                    var getEnumeratorMethod = enumerableType.GetMethod("GetEnumerator");
                    
                    if (getEnumeratorMethod == null)
                    {
                        Debug.LogError("[ARM Tracker] GetEnumerator 메서드를 찾을 수 없습니다.");
                        return;
                    }
                    
                    var enumerator = getEnumeratorMethod.Invoke(entriesCollection, null);
                    var enumeratorType = enumerator.GetType();
                    var moveNextMethod = enumeratorType.GetMethod("MoveNext");
                    var currentProperty = enumeratorType.GetProperty("Current");
                    
                    if (moveNextMethod == null || currentProperty == null)
                    {
                        Debug.LogError("[ARM Tracker] 열거자 메서드를 찾을 수 없습니다.");
                        return;
                    }
                    
                    int entryCount = 0;
                    
                    while ((bool)moveNextMethod.Invoke(enumerator, null))
                    {
                        var originalEntry = currentProperty.GetValue(enumerator);
                        if (originalEntry != null)
                        {
                            try
                            {
                                var proxyEntry = new AssetEntryProxy(originalEntry);
                                _allEntries.Add(proxyEntry);
                                entryCount++;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[ARM Tracker] 에셋 엔트리 래핑 중 오류: {ex.Message}");
                            }
                        }
                    }
                    
                    // 필터와 정렬 적용
                    ApplyVisibilityFilters(_showBatchLoaded, _showIndividualLoaded);
                    ApplyFilter(_currentFilter);
                    ApplySorting(_sortIndex);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ARM Tracker] 에셋 엔트리 처리 중 오류: {e.Message}\n{e.StackTrace}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ARM Tracker] ARM Tracker 데이터 새로고침 중 오류: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public void ApplyFilter(string filter)
        {
            _currentFilter = filter;
            UpdateVisibleEntries();
        }
        
        public void ApplyVisibilityFilters(bool showBatchLoaded, bool showIndividualLoaded)
        {
            _showBatchLoaded = showBatchLoaded;
            _showIndividualLoaded = showIndividualLoaded;
            UpdateVisibleEntries();
        }
        
        public void ApplySorting(int sortIndex)
        {
            _sortIndex = sortIndex;
            UpdateVisibleEntries();
        }
        
        private void UpdateVisibleEntries()
        {
            _visibleEntries.Clear();
            
            // 필터 적용
            IEnumerable<AssetEntryProxy> filtered = _allEntries;
            
            // 배치/개별 로딩 필터
            filtered = filtered.Where(e => 
                (_showBatchLoaded && e.IsBatchLoaded) || 
                (_showIndividualLoaded && !e.IsBatchLoaded));
            
            // 검색 텍스트 필터
            if (!string.IsNullOrEmpty(_currentFilter))
            {
                string lowerFilter = _currentFilter.ToLowerInvariant();
                filtered = filtered.Where(e => e.Key.ToLowerInvariant().Contains(lowerFilter));
            }
            
            // 정렬 적용
            IOrderedEnumerable<AssetEntryProxy> sorted;
            
            switch (_sortIndex)
            {
                case 0: // 키 이름
                    sorted = filtered.OrderBy(e => e.Key);
                    break;
                case 1: // 참조 카운트 (높은 순)
                    sorted = filtered.OrderByDescending(e => e.ReferenceCount);
                    break;
                case 2: // 참조 카운트 (낮은 순)
                    sorted = filtered.OrderBy(e => e.ReferenceCount);
                    break;
                case 3: // 배치 로드 우선
                    sorted = filtered.OrderByDescending(e => e.IsBatchLoaded).ThenBy(e => e.Key);
                    break;
                case 4: // 개별 로드 우선
                    sorted = filtered.OrderBy(e => e.IsBatchLoaded).ThenBy(e => e.Key);
                    break;
                case 5: // 핸들 개수 (높은 순)
                    sorted = filtered.OrderByDescending(e => e.HandleMap.Count);
                    break;
                default:
                    sorted = filtered.OrderBy(e => e.Key);
                    break;
            }
            
            _visibleEntries.AddRange(sorted);
        }
        
        public List<string> FindReferences(AssetEntryProxy entry)
        {
            if (_referenceCache.TryGetValue(entry, out var cachedReferences))
            {
                return cachedReferences;
            }
            
            var references = new List<string>();
            
            // 에셋의 참조 정보 수집
            try
            {
                // 현재 액티브 씬의 GameObject들을 검사
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded)
                    {
                        references.Add($"Scene: {scene.name}");
                        
                        foreach (var rootObj in scene.GetRootGameObjects())
                        {
                            ScanGameObjectForReferences(rootObj, entry, references);
                        }
                    }
                }
                
                // 참조 카운트 정보 추가
                if (entry.ReferenceCount > 0)
                {
                    references.Insert(0, $"Reference Count: {entry.ReferenceCount}");
                }
                
                // 핸들 정보 추가
                if (entry.HandleMap.Count > 0)
                {
                    references.Insert(1, $"Handle Count: {entry.HandleMap.Count}");
                }
                
                // 참조를 찾지 못한 경우 메시지 추가
                if (references.Count <= 2)
                {
                    if (entry.ReferenceCount > 0)
                    {
                        references.Add($"Asset is referenced {entry.ReferenceCount} times, but specific objects could not be determined.");
                    }
                    else
                    {
                        references.Add("No active references found (Reference Count is 0).");
                    }
                }
            }
            catch (Exception e)
            {
                references.Add($"Error finding references: {e.Message}");
                Debug.LogError($"FindReferences 오류: {e.Message}\n{e.StackTrace}");
            }
            
            _referenceCache[entry] = references;
            return references;
        }
        
        private void ScanGameObjectForReferences(GameObject obj, AssetEntryProxy entry, List<string> references)
        {
            // 컴포넌트 스캔 로직 - 실제로는 런타임에서 참조를 정확히 추적하기 어려움
            // 여기서는 이름 비교나 간단한 휴리스틱만 사용
            
            if (obj.name.Contains(entry.Key))
            {
                references.Add($"GameObject: {GetGameObjectPath(obj)}");
            }
            
            // 자식 객체 재귀적으로 검사
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                ScanGameObjectForReferences(obj.transform.GetChild(i).gameObject, entry, references);
            }
        }
        
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
    }
}