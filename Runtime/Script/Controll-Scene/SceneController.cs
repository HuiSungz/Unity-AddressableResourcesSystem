
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace AddressableManage
{
    internal partial class SceneController : ISceneController
    {
        [DependencyInject] private IAssetRegistry _registry;
        
        private const float MIN_PROGRESS = 0f;
        private const float MAX_PROGRESS = 1.0f;

        public SceneController()
        {
            DependencyContainer.Instance.InjectDependencies(this);
        }
        
        public ARMOperationHandle<SceneEntry> LoadScene(
            string sceneKey, 
            LoadSceneMode loadMode = LoadSceneMode.Additive, 
            bool activateOnLoad = true)
        {
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. You should call ARM.Initialize() first.");
                return new ARMOperationHandle<SceneEntry>();
            }
            
            var armOperationHandle = new ARMOperationHandle<SceneEntry>();
            if (TryGetLoadedEntry(sceneKey, out var existingScene))
            {
                armOperationHandle.Complete(existingScene);
                return armOperationHandle;
            }
            
            _ = LoadSceneAsync(sceneKey, loadMode, activateOnLoad, armOperationHandle);
            
            return armOperationHandle;
        }
        
        public ARMOperationHandle<SceneEntry> LoadScene(
            AssetReference sceneReference, 
            LoadSceneMode loadMode = LoadSceneMode.Additive, 
            bool activateOnLoad = true)
        {
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. You should call ARM.Initialize() first.");
                return new ARMOperationHandle<SceneEntry>();
            }
            
            if (sceneReference == null)
            {
                Verbose.E("Scene reference is null.");
                var failedHandle = new ARMOperationHandle<SceneEntry>();
                failedHandle.Fail(new ArgumentNullException(nameof(sceneReference)));
                return failedHandle;
            }
            
            var armOperationHandle = new ARMOperationHandle<SceneEntry>();
            
            _ = LoadSceneAsync(sceneReference, loadMode, activateOnLoad, armOperationHandle);
            
            return armOperationHandle;
        }

        public ARMOperationHandle<bool> UnloadScene(string sceneKey)
        {
            var armOperationHandle = new ARMOperationHandle<bool>();
            
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. You should call ARM.Initialize() first.");
                armOperationHandle.Fail(new Exception("ARM not initialized"));
                return armOperationHandle;
            }
            
            if (!TryGetLoadedEntry(sceneKey, out var sceneEntry))
            {
                Verbose.W($"Scene {sceneKey} is not loaded or not found.");
                armOperationHandle.Complete(false);
                return armOperationHandle;
            }
            
            _ = UnloadSceneAsync(sceneEntry, armOperationHandle);
            
            return armOperationHandle;
        }
        
        public ARMOperationHandle<bool> UnloadScene(AssetReference sceneReference)
        {
            var armOperationHandle = new ARMOperationHandle<bool>();
            
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. You should call ARM.Initialize() first.");
                armOperationHandle.Fail(new Exception("ARM not initialized"));
                return armOperationHandle;
            }
            
            if (sceneReference == null)
            {
                Verbose.E("Scene reference is null.");
                armOperationHandle.Fail(new ArgumentNullException(nameof(sceneReference)));
                return armOperationHandle;
            }
            
            if (!TryGetLoadedEntry(sceneReference, out var sceneEntry))
            {
                Verbose.W($"Scene for reference {sceneReference} is not loaded or not found.");
                armOperationHandle.Complete(false);
                return armOperationHandle;
            }
            
            _ = UnloadSceneAsync(sceneEntry, armOperationHandle);
            
            return armOperationHandle;
        }
        
        public ARMOperationHandle<bool> UnloadScene(SceneEntry sceneEntry)
        {
            var armOperationHandle = new ARMOperationHandle<bool>();
            
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. You should call ARM.Initialize() first.");
                armOperationHandle.Fail(new Exception("ARM not initialized"));
                return armOperationHandle;
            }
            
            if (sceneEntry is not { IsValid: true })
            {
                Verbose.W("Scene entry is null or not valid.");
                armOperationHandle.Complete(false);
                return armOperationHandle;
            }
            
            _ = UnloadSceneAsync(sceneEntry, armOperationHandle);
            
            return armOperationHandle;
        }
        
        public ARMOperationHandle<bool> ActivateScene(SceneEntry sceneEntry)
        {
            var armOperationHandle = new ARMOperationHandle<bool>();
            
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. You should call ARM.Initialize() first.");
                armOperationHandle.Fail(new Exception("ARM not initialized"));
                return armOperationHandle;
            }
            
            if (sceneEntry is not { IsValid: true })
            {
                Verbose.W("Scene entry is null or not valid.");
                armOperationHandle.Fail(new ArgumentException("Invalid scene entry"));
                return armOperationHandle;
            }
            
            _ = ActivateSceneAsync(sceneEntry, armOperationHandle);
            
            return armOperationHandle;
        }
        
        public bool TryGetLoadedEntry(string sceneKey, out SceneEntry sceneEntry)
        {
            sceneEntry = null;
            
            if (_registry.Scenes.TryGetValue(sceneKey, out sceneEntry))
            {
                return true;
            }
            
            try
            {
                var locationHandle = Addressables.LoadResourceLocationsAsync(sceneKey);
                locationHandle.WaitForCompletion();
                
                if (locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result.Count == 0)
                {
                    locationHandle.Release();
                    return false;
                }
                
                var primaryKey = locationHandle.Result[0].PrimaryKey;
                locationHandle.Release();
                
                return _registry.Scenes.TryGetValue(primaryKey, out sceneEntry);
            }
            catch (Exception ex)
            {
                Verbose.Ex($"Error in TryGetLoadedEntry for {sceneKey}", ex);
                return false;
            }
        }
        
        public bool TryGetLoadedEntry(AssetReference sceneReference, out SceneEntry sceneEntry)
        {
            sceneEntry = null;
            
            if (sceneReference == null)
            {
                Verbose.E("Scene reference is null.");
                return false;
            }
            
            try
            {
                var locationHandle = Addressables.LoadResourceLocationsAsync(sceneReference);
                locationHandle.WaitForCompletion();
                
                if (locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result.Count == 0)
                {
                    locationHandle.Release();
                    return false;
                }
                
                var primaryKey = locationHandle.Result[0].PrimaryKey;
                locationHandle.Release();
                
                return _registry.Scenes.TryGetValue(primaryKey, out sceneEntry);
            }
            catch (Exception ex)
            {
                Verbose.Ex($"Error in TryGetLoadedEntry for AssetReference {sceneReference}", ex);
                return false;
            }
        }
        
        public async UniTask<bool> UnloadAllScenes()
        {
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. You should call ARM.Initialize() first.");
                return false;
            }
            
            try
            {
                var sceneEntries = _registry.Scenes.Values.ToList();
                foreach (var entry in sceneEntries)
                {
                    if (!entry.IsValid)
                    {
                        continue;
                    }
                    
                    if (sceneEntries.Count == 1 && entry.SceneMode == LoadSceneMode.Single)
                    {
                        Verbose.W("Skipping unload of the only single scene to prevent empty scene hierarchy.");
                        continue;
                    }
                    
                    var unloadHandle = UnloadScene(entry);
                    await unloadHandle.AsUniTask();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Verbose.Ex("Failed to unload all scenes", ex);
                return false;
            }
        }
    }
}