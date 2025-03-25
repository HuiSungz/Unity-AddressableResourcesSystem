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
    /// Wrapper class for asset entries, using reflection to access internal data.
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
                // Extract Key - try field first, then property
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
                
                // Extract IsBatchLoaded - try field first, then property
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
                
                // Extract field: ReferenceCount
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
                Debug.LogError($"[ARM Tracker] Error extracting asset entry properties: {ex.Message}\n{ex.StackTrace}");
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
                // Extract field: ReferenceCount
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
                Debug.LogError($"[ARM Tracker] Error refreshing asset entry data: {ex.Message}");
            }
        }
    }

    public class ARMTrackerReflectionData
    {
        // Existing data
        private readonly List<AssetEntryProxy> _allEntries = new List<AssetEntryProxy>();
        private readonly List<AssetEntryProxy> _visibleEntries = new List<AssetEntryProxy>();
        private readonly Dictionary<AssetEntryProxy, List<string>> _referenceCache = new Dictionary<AssetEntryProxy, List<string>>();
        
        // Filter state
        private string _currentFilter = "";
        private bool _showBatchLoaded = true;
        private bool _showIndividualLoaded = true;
        private int _sortIndex = 0;
        
        // Statistics
        public int TotalAssetCount => _allEntries.Count;
        public int BatchLoadedCount => _allEntries.Count(e => e.IsBatchLoaded);
        public int IndividualLoadedCount => _allEntries.Count(e => !e.IsBatchLoaded);
        
        // Cached reflection info
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
        
        public void InitializeReflectionInfo()
        {
            try
            {
                // Find the ARM class in all assemblies
                _armType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // Skip editor assemblies
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
                            Debug.Log($"[ARM Tracker] Found ARM type: {_armType.FullName}, Assembly: {assembly.GetName().Name}");
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // Some assemblies might fail GetTypes() - ignore and continue
                        continue;
                    }
                }
                
                if (_armType == null)
                {
                    // Fallback: attempt by specific assembly name
                    try 
                    {
                        var mainAssembly = Assembly.Load("Assembly-CSharp");
                        _armType = mainAssembly.GetType("AddressableManage.ARM");
                        Debug.Log($"[ARM Tracker] Found ARM type using fallback: {_armType?.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ARM Tracker] Unable to find ARM type. Error: {ex.Message}");
                        return;
                    }
                }
                
                if (_armType == null)
                {
                    Debug.LogError("[ARM Tracker] ARM type not found.");
                    return;
                }
                
                // Find Instance property (try Public and NonPublic)
                _instanceProperty = _armType.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    
                if (_instanceProperty == null)
                {
                    // Fallback: try finding another static property or field for singleton pattern
                    var staticProperties = _armType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    var staticFields = _armType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    
                    // Prefer a static property or field whose type matches the ARM type
                    foreach (var prop in staticProperties)
                    {
                        if (prop.PropertyType == _armType)
                        {
                            _instanceProperty = prop;
                            Debug.Log($"[ARM Tracker] Found alternative singleton property: {prop.Name}");
                            break;
                        }
                    }
                    
                    if (_instanceProperty == null)
                    {
                        // Check static fields
                        foreach (var field in staticFields)
                        {
                            if (field.FieldType == _armType)
                            {
                                // Use the field as a fallback
                                _instanceField = field;
                                Debug.Log($"[ARM Tracker] Found alternative singleton field: {field.Name}");
                                break;
                            }
                        }
                    }
                }
                
                if (_instanceProperty == null && _instanceField == null)
                {
                    Debug.LogError("[ARM Tracker] Unable to find singleton instance of ARM.");
                    return;
                }
                
                // Find _registry field
                _registryField = _armType.GetField("_registry", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_registryField == null)
                {
                    // Fallback: try another field name containing "registry" or "asset"
                    var candidateFields = _armType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => f.Name.ToLower().Contains("registry") || f.Name.ToLower().Contains("asset"))
                        .ToArray();
                        
                    if (candidateFields.Length > 0)
                    {
                        _registryField = candidateFields[0];
                        Debug.Log($"[ARM Tracker] Found alternative registry field: {_registryField.Name}");
                    }
                }
                
                if (_registryField == null)
                {
                    Debug.LogError("[ARM Tracker] Unable to find _registry field in ARM.");
                    return;
                }
                
                // Get registry type
                _registryType = _registryField.FieldType;
                Debug.Log($"[ARM Tracker] Registry type: {_registryType.FullName}");
                
                // Find Assets property
                _assetsProperty = _registryType.GetProperty("Assets");
                if (_assetsProperty == null)
                {
                    // Fallback: try another property or field name for a dictionary of assets
                    var candidateProps = _registryType.GetProperties()
                        .Where(p => p.Name.ToLower().Contains("asset") && 
                               p.PropertyType.IsGenericType && 
                               p.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        .ToArray();
                        
                    if (candidateProps.Length > 0)
                    {
                        _assetsProperty = candidateProps[0];
                        Debug.Log($"[ARM Tracker] Found alternative Assets property: {_assetsProperty.Name}");
                    }
                    else
                    {
                        // Also check for a field
                        var candidateFields = _registryType.GetFields()
                            .Where(f => f.Name.ToLower().Contains("asset") && 
                                   f.FieldType.IsGenericType && 
                                   f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                            .ToArray();
                            
                        if (candidateFields.Length > 0)
                        {
                            _assetsField = candidateFields[0];
                            Debug.Log($"[ARM Tracker] Found alternative Assets field: {_assetsField.Name}");
                        }
                    }
                }
                
                if (_assetsProperty == null && _assetsField == null)
                {
                    Debug.LogError("[ARM Tracker] Unable to find Assets collection.");
                    return;
                }
                
                Debug.Log("[ARM Tracker] Reflection initialization completed successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ARM Tracker] Error during reflection initialization: {e.Message}\n{e.StackTrace}");
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
            
            // Check if in play mode
            if (!EditorApplication.isPlaying)
            {
                Debug.Log("[ARM Tracker] Data can only be queried in play mode.");
                return;
            }
            
            try
            {
                // Check if ARM is initialized
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
                    // The Initialized property might not exist - continue
                }
                
                if (!armInitialized)
                {
                    Debug.LogWarning("[ARM Tracker] ARM is not initialized or could not be verified. Proceeding anyway.");
                }
                
                // Get ARM instance
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
                    Debug.LogWarning("[ARM Tracker] Could not find ARM instance. Please verify if ARM is initialized.");
                    return;
                }
                
                // Get AssetRegistry instance
                object registryInstance = _registryField.GetValue(armInstance);
                if (registryInstance == null)
                {
                    Debug.LogError("[ARM Tracker] Could not find AssetRegistry instance.");
                    return;
                }
                
                // Get Assets dictionary
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
                    Debug.LogError("[ARM Tracker] Could not access Assets collection.");
                    return;
                }
                
                if (assetsDictionary == null)
                {
                    Debug.LogWarning("[ARM Tracker] Assets dictionary is null.");
                    return;
                }
                
                // Get the Values property of the dictionary
                try
                {
                    var dictionaryType = assetsDictionary.GetType();
                    var valuesProperty = dictionaryType.GetProperty("Values");
                    
                    if (valuesProperty == null)
                    {
                        Debug.LogError("[ARM Tracker] Unable to find the Values property of the dictionary.");
                        return;
                    }
                    
                    var entriesCollection = valuesProperty.GetValue(assetsDictionary);
                    
                    if (entriesCollection == null)
                    {
                        Debug.LogWarning("[ARM Tracker] Asset entry collection is null.");
                        return;
                    }
                    
                    // Ensure the collection is enumerable (implements IEnumerable)
                    var enumerableType = entriesCollection.GetType();
                    var getEnumeratorMethod = enumerableType.GetMethod("GetEnumerator");
                    
                    if (getEnumeratorMethod == null)
                    {
                        Debug.LogError("[ARM Tracker] Could not find GetEnumerator method.");
                        return;
                    }
                    
                    var enumerator = getEnumeratorMethod.Invoke(entriesCollection, null);
                    var enumeratorType = enumerator.GetType();
                    var moveNextMethod = enumeratorType.GetMethod("MoveNext");
                    var currentProperty = enumeratorType.GetProperty("Current");
                    
                    if (moveNextMethod == null || currentProperty == null)
                    {
                        Debug.LogError("[ARM Tracker] Unable to find enumerator methods.");
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
                                Debug.LogWarning($"[ARM Tracker] Error wrapping asset entry: {ex.Message}");
                            }
                        }
                    }
                    
                    // Apply filters and sorting
                    ApplyVisibilityFilters(_showBatchLoaded, _showIndividualLoaded);
                    ApplyFilter(_currentFilter);
                    ApplySorting(_sortIndex);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ARM Tracker] Error processing asset entries: {e.Message}\n{e.StackTrace}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ARM Tracker] Error refreshing ARM Tracker data: {e.Message}\n{e.StackTrace}");
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
            
            // Apply filters
            IEnumerable<AssetEntryProxy> filtered = _allEntries;
            
            // Batch/Individual loading filter
            filtered = filtered.Where(e => 
                (_showBatchLoaded && e.IsBatchLoaded) || 
                (_showIndividualLoaded && !e.IsBatchLoaded));
            
            // Search text filter
            if (!string.IsNullOrEmpty(_currentFilter))
            {
                string lowerFilter = _currentFilter.ToLowerInvariant();
                filtered = filtered.Where(e => e.Key.ToLowerInvariant().Contains(lowerFilter));
            }
            
            // Apply sorting
            IOrderedEnumerable<AssetEntryProxy> sorted;
            
            switch (_sortIndex)
            {
                case 0: // Key name
                    sorted = filtered.OrderBy(e => e.Key);
                    break;
                case 1: // Reference count (descending)
                    sorted = filtered.OrderByDescending(e => e.ReferenceCount);
                    break;
                case 2: // Reference count (ascending)
                    sorted = filtered.OrderBy(e => e.ReferenceCount);
                    break;
                case 3: // Batch loaded first
                    sorted = filtered.OrderByDescending(e => e.IsBatchLoaded).ThenBy(e => e.Key);
                    break;
                case 4: // Individually loaded first
                    sorted = filtered.OrderBy(e => e.IsBatchLoaded).ThenBy(e => e.Key);
                    break;
                case 5: // Handle count (descending)
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
            
            // Collect asset reference information
            try
            {
                // Scan GameObjects in active scenes
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
                
                // Add reference count info
                if (entry.ReferenceCount > 0)
                {
                    references.Insert(0, $"Reference Count: {entry.ReferenceCount}");
                }
                
                // Add handle info
                if (entry.HandleMap.Count > 0)
                {
                    references.Insert(1, $"Handle Count: {entry.HandleMap.Count}");
                }
                
                // If no specific references found, add a message
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
                Debug.LogError($"Error in FindReferences: {e.Message}\n{e.StackTrace}");
            }
            
            _referenceCache[entry] = references;
            return references;
        }
        
        private void ScanGameObjectForReferences(GameObject obj, AssetEntryProxy entry, List<string> references)
        {
            // Component scanning logic - in reality, accurately tracking references at runtime is challenging.
            // Here, we use name comparison or simple heuristics.
            
            if (obj.name.Contains(entry.Key))
            {
                references.Add($"GameObject: {GetGameObjectPath(obj)}");
            }
            
            // Recursively scan child objects
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