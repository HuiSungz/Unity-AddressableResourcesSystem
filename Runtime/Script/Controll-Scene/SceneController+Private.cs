
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AddressableManage
{
    internal partial class SceneController
    {
        private async UniTask LoadSceneAsync(
            string sceneKey, 
            LoadSceneMode loadMode, 
            bool activateOnLoad, 
            ARMOperationHandle<SceneEntry> armOperationHandle)
        {
            try
            {
                armOperationHandle.Progress = MIN_PROGRESS;
                var locations = await LoadLocationAsync(sceneKey);
                if (locations == null || locations.Count == 0)
                {
                    armOperationHandle.Fail(new Exception($"Failed to find scene location for key: {sceneKey}"));
                    return;
                }
                
                armOperationHandle.Progress = 0.1f;
                var primaryKey = locations[0].PrimaryKey;
                if (TryGetLoadedEntry(primaryKey, out var existingScene))
                {
                    armOperationHandle.Complete(existingScene);
                    return;
                }
                
                await LoadSceneWithTracking(primaryKey, loadMode, activateOnLoad, armOperationHandle);
            }
            catch (Exception ex)
            {
                Verbose.Ex($"Failed to load scene: {sceneKey}", ex);
                armOperationHandle.Fail(ex);
            }
        }
        
        private async UniTask LoadSceneAsync(
            AssetReference sceneReference, 
            LoadSceneMode loadMode, 
            bool activateOnLoad, 
            ARMOperationHandle<SceneEntry> armOperationHandle)
        {
            try
            {
                armOperationHandle.Progress = MIN_PROGRESS;
                var locations = await LoadLocationAsync(sceneReference);
                if (locations == null || locations.Count == 0)
                {
                    armOperationHandle.Fail(new Exception($"Failed to find scene location for reference: {sceneReference}"));
                    return;
                }
                
                armOperationHandle.Progress = 0.1f;

                var primaryKey = locations[0].PrimaryKey;
                if (TryGetLoadedEntry(primaryKey, out var existingScene))
                {
                    armOperationHandle.Complete(existingScene);
                    return;
                }

                await LoadSceneWithTracking(primaryKey, loadMode, activateOnLoad, armOperationHandle);
            }
            catch (Exception ex)
            {
                Verbose.Ex($"Failed to load scene from reference: {sceneReference}", ex);
                armOperationHandle.Fail(ex);
            }
        }
        
        private async UniTask LoadSceneWithTracking(
            string sceneKey, 
            LoadSceneMode loadMode, 
            bool activateOnLoad, 
            ARMOperationHandle<SceneEntry> armOperationHandle)
        {
            var loadSceneHandle = default(AsyncOperationHandle<SceneInstance>);
            try
            {
                var sceneEntry = new SceneEntry(sceneKey, loadMode);
                loadSceneHandle = Addressables.LoadSceneAsync(sceneKey, loadMode, activateOnLoad);
                
                await MonitorLoadingProgress(loadSceneHandle, armOperationHandle, 0.1f, 0.9f);
                await loadSceneHandle.ToUniTask();
                sceneEntry.SceneHandle = loadSceneHandle;
                
                _registry.Scenes[sceneKey] = sceneEntry;
                
                Verbose.D($"Scene loaded: {sceneKey}, Mode: {loadMode}, Activated: {activateOnLoad}");
                
                armOperationHandle.Progress = MAX_PROGRESS;
                armOperationHandle.Complete(sceneEntry);
            }
            catch (Exception exception)
            {
                Verbose.Ex($"LoadSceneWithTracking failed for {sceneKey}", exception);
                if (loadSceneHandle.IsValid())
                {
                    await Addressables.UnloadSceneAsync(loadSceneHandle);
                }
                
                armOperationHandle.Fail(exception);
            }
        }
        
        private async UniTask UnloadSceneAsync(SceneEntry sceneEntry, ARMOperationHandle<bool> armOperationHandle)
        {
            try
            {
                armOperationHandle.Progress = MIN_PROGRESS;
                
                var sceneKey = sceneEntry.Key;
                
                _registry.Scenes.Remove(sceneKey);
                
                var unloadHandle = Addressables.UnloadSceneAsync(sceneEntry.SceneHandle);
                
                await MonitorLoadingProgress(unloadHandle, armOperationHandle, 0f, 1f);
                await unloadHandle.ToUniTask();
                
                Verbose.D($"Scene unloaded: {sceneKey}");
                
                armOperationHandle.Progress = MAX_PROGRESS;
                armOperationHandle.Complete(true);
            }
            catch (Exception ex)
            {
                Verbose.Ex($"Failed to unload scene: {sceneEntry.Key}", ex);
                armOperationHandle.Fail(ex);
            }
        }
        
        private async UniTask ActivateSceneAsync(SceneEntry sceneEntry, ARMOperationHandle<bool> armOperationHandle)
        {
            try
            {
                armOperationHandle.Progress = MIN_PROGRESS;
                
                var activationOperation = sceneEntry.SceneInstance.ActivateAsync();
                
                float progress = 0f;
                while (!activationOperation.isDone)
                {
                    progress = Mathf.Min(progress + 0.05f, 0.9f);
                    armOperationHandle.Progress = progress;
                    
                    await UniTask.Yield();
                }
                
                await UniTask.WaitUntil(() => activationOperation.isDone);
                
                Verbose.D($"Scene activated: {sceneEntry.Key}");
                
                armOperationHandle.Progress = MAX_PROGRESS;
                armOperationHandle.Complete(true);
            }
            catch (Exception ex)
            {
                Verbose.Ex($"Failed to activate scene: {sceneEntry.Key}", ex);
                armOperationHandle.Fail(ex);
            }
        }
        
        private async UniTask<IList<IResourceLocation>> LoadLocationAsync(object key)
        {
            AsyncOperationHandle<IList<IResourceLocation>> operationHandle = default;
            
            try
            {
                operationHandle = Addressables.LoadResourceLocationsAsync(key);
                return await operationHandle.ToUniTask();
            }
            catch (Exception ex)
            {
                Verbose.Ex($"LoadLocationAsync Error for {key}", ex);
                return new List<IResourceLocation>();
            }
            finally
            {
                if (operationHandle.IsValid())
                {
                    operationHandle.Release();
                }
            }
        }
        
        private async UniTask MonitorLoadingProgress<T>(
            AsyncOperationHandle asyncOp, 
            ARMOperationHandle<T> armOperationHandle,
            float startProgress,
            float progressWeight)
        {
            var lastPercent = 0f;
            
            while (!asyncOp.IsDone)
            {
                if (asyncOp.PercentComplete > lastPercent)
                {
                    lastPercent = asyncOp.PercentComplete;
                    var progress = startProgress + (lastPercent * progressWeight);
                    armOperationHandle.Progress = Math.Min(progress, MAX_PROGRESS);
                }
                
                await UniTask.Yield();
            }
        }
    }
}