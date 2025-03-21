
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AddressableManage
{
    internal class AssetIndividualLoader : AssetLoaderBase
    {
        #region Load

        public UniTask<AssetEntry> LoadAsync(string key, ARMOperationHandle<AssetEntry> armOperationHandle)
            => LoadAsync<string>(key, armOperationHandle);

        public UniTask<AssetEntry> LoadAsync(AssetReference key, ARMOperationHandle<AssetEntry> armOperationHandle)
            => LoadAsync<AssetReference>(key, armOperationHandle);
        
        private async UniTask<AssetEntry> LoadAsync<TKey>(TKey key, ARMOperationHandle<AssetEntry> armOperationHandle)
        {
            try
            {
                armOperationHandle.Progress = 0f;
                var assetEntry = key switch
                {
                    string stringKey => await LoadAsyncInternal(stringKey, armOperationHandle),
                    AssetReference assetRef => await LoadAsyncInternal(assetRef, armOperationHandle),
                    _ => throw new ArgumentException($"Unsupported key type: {typeof(TKey).Name}")
                };
                
                if (assetEntry != null)
                {
                    armOperationHandle.Complete(assetEntry);
                    return assetEntry;
                }

                armOperationHandle.Fail(new Exception($"Failed to load asset with key {key}"));
                return null;
            }
            catch (Exception exception)
            {
                Verbose.Ex($"IndividualLoader LoadAsync Failed for key {key}.", exception);
                armOperationHandle.Fail(exception);
                return null;
            }
        }

        private async UniTask<AssetEntry> LoadAsyncInternal(AssetReference key, ARMOperationHandle<AssetEntry> armOperationHandle)
        {
            try
            {
                var locations = await LoadLocationAsync(key);
                if (locations == null || locations.Count == 0)
                {
                    Verbose.E($"IndividualLoader Error: Failed to load locations for AssetReference {key}");
                    return null;
                }
                
                armOperationHandle.Progress = 0.1f;
                
                var primaryKey = locations[0].PrimaryKey;
                if (_registry.TryGetAsset(primaryKey, out var existingEntry))
                {
                    await ProcessMissingLocations(locations, existingEntry, armOperationHandle);
                    return existingEntry;
                }
                
                var assetEntry = await ProcessLocations(locations, primaryKey, armOperationHandle);
                return assetEntry;
            }
            catch (Exception exception)
            {
                Verbose.Ex($"IndividualLoader LoadAsync Failed for AssetReference {key}.", exception);
                return null;
            }
        }

        private async UniTask<AssetEntry> LoadAsyncInternal(string key, ARMOperationHandle<AssetEntry> armOperationHandle)
        {
            try
            {
                var locations = await LoadLocationAsync(key);
                if (locations == null || locations.Count == 0)
                {
                    Verbose.E($"IndividualLoader Error: Failed to load locations for key {key}");
                    return null;
                }
                
                armOperationHandle.Progress = 0.1f;
                
                if (_registry.TryGetAsset(key, out var existingEntry))
                {
                    await ProcessMissingLocations(locations, existingEntry, armOperationHandle);
                    return existingEntry;
                }
                
                var assetEntry = await ProcessLocations(locations, key, armOperationHandle);
                return assetEntry;
            }
            catch (Exception exception)
            {
                Verbose.Ex($"IndividualLoader LoadAsync Failed for key {key}.", exception);
                return null;
            }
        }

        #endregion

        private async UniTask ProcessMissingLocations(
            IList<IResourceLocation> allLocations, 
            AssetEntry existingEntry, 
            ARMOperationHandle<AssetEntry> armOperationHandle)
        {
            var loadedLocationsSet = new HashSet<IResourceLocation>(
                existingEntry.HandleMap.Keys, 
                ResourceLocationEqualityComparer.Instance);
            var missingLocations = new List<IResourceLocation>();
            foreach (var location in allLocations)
            {
                if (!loadedLocationsSet.Contains(location))
                {
                    missingLocations.Add(location);
                }
            }
            
            if (missingLocations.Count == 0)
            {
                Verbose.D($"IndividualLoader: All locations are already loaded for {existingEntry.Key}");
                armOperationHandle.Progress = 1.0f;
                return;
            }
            
            Verbose.D($"IndividualLoader: Found {missingLocations.Count} missing locations for {existingEntry.Key}");
            
            var progressStep = 0.9f / missingLocations.Count;
            var currentProgress = 0.1f;
            
            foreach (var location in missingLocations)
            {
                await ProcessLocationForEntry(location, existingEntry, armOperationHandle);
                currentProgress += progressStep;
                armOperationHandle.Progress = currentProgress;
            }
            
            armOperationHandle.Progress = 1.0f;
        }

        private async UniTask<AssetEntry> ProcessLocations(
            IList<IResourceLocation> resourceLocations, 
            string key, 
            ARMOperationHandle<AssetEntry> armOperationHandle)
        {
            if (!_registry.TryGetAsset(key, out var resultEntry))
            {
                resultEntry = new AssetEntry(key, false);
                _registry.AddAsset(key, resultEntry);
            }

            var progressStep = 0.9f / resourceLocations.Count;
            var currentProgress = 0.1f;
            
            foreach (var location in resourceLocations)
            {
                await ProcessLocationForEntry(location, resultEntry, armOperationHandle);
                currentProgress += progressStep;
                armOperationHandle.Progress = currentProgress;
            }
            
            armOperationHandle.Progress = 1.0f;
            return resultEntry;
        }
        
        private async UniTask ProcessLocationForEntry(
            IResourceLocation resourceLocation, 
            AssetEntry assetEntry, 
            ARMOperationHandle<AssetEntry> armOperationHandle)
        {
            try
            {
                foreach (var existingLocation in assetEntry.HandleMap.Keys)
                {
                    if (!ResourceLocationEqualityComparer.Instance.Equals(existingLocation, resourceLocation))
                    {
                        continue;
                    }
                    
                    Verbose.D($"IndividualLoader: Location already loaded for {assetEntry.Key}. Skipping. Location: {resourceLocation.PrimaryKey}");
                    return;
                }
                
                var operationHandle = TypedAssetLoader.LoadAssetByType(resourceLocation);
                await MonitorLoadingProgress(operationHandle, armOperationHandle, armOperationHandle.Progress, 0.1f);
                await operationHandle.ToUniTask();
                
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    var duplicateFound = false;
                    foreach (var existingLocation in assetEntry.HandleMap.Keys)
                    {
                        if (!ResourceLocationEqualityComparer.Instance.Equals(existingLocation, resourceLocation))
                        {
                            continue;
                        }
                        
                        duplicateFound = true;
                        Verbose.D($"IndividualLoader: Location was loaded during async wait for {assetEntry.Key}. Releasing duplicate.");
                        if (operationHandle.IsValid())
                        {
                            operationHandle.Release();
                        }
                        break;
                    }

                    if (!duplicateFound)
                    {
                        Verbose.D($"IndividualLoader: Adding handle to map for {assetEntry.Key}, ResourceType: {resourceLocation.ResourceType?.Name}");
                        assetEntry.HandleMap[resourceLocation] = operationHandle;
                        HandleTrackingSystem.TrackHandle(assetEntry.Key, operationHandle);
                    }
                }
                else
                {
                    Verbose.W($"IndividualLoader: Failed to load asset for {assetEntry.Key}. Status: {operationHandle.Status}");
                    if (operationHandle.IsValid())
                    {
                        operationHandle.Release();
                    }
                }
            }
            catch (Exception exception)
            {
                Verbose.Ex($"IndividualLoader ProcessLocationForEntry Failed for {resourceLocation.PrimaryKey}.", exception);
            }
        }
    }
}