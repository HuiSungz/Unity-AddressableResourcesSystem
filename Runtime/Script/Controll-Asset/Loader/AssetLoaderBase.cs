
#if ARM_UNITASK
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ArchitectHS.AddressableManage
{
    internal abstract class AssetLoaderBase
    {
        #region Fields & Constructor

        protected const float MIN_PROGRESS = 0f;
        protected const float MAX_PROGRESS = 1.0f;
        
        [DependencyInject] protected IAssetRegistry _registry;
        
        protected AssetLoaderBase()
        {
            DependencyContainer.Instance.InjectDependencies(this);
        }

        #endregion
        
        protected async UniTask<IList<IResourceLocation>> LoadLocationAsync(object key)
        {
            AsyncOperationHandle<IList<IResourceLocation>> operationHandle = default;
            
            try
            {
                operationHandle = Addressables.LoadResourceLocationsAsync(key);
                return await operationHandle.ToUniTask();
            }
            catch (Exception exception)
            {
                Verbose.Ex("LoadLocationAsyncInternal Error : ", exception);
                return _registry.NoLocations;
            }
            finally
            {
                if (operationHandle.IsValid())
                {
                    operationHandle.Release();
                }
            }
        }
        
        protected bool IsAssetAlreadyLoaded(IResourceLocation resourceLocation)
        {
            var primaryKey = resourceLocation.PrimaryKey;
            if (!_registry.TryGetAsset(primaryKey, out var existingEntry))
            {
                return false;
            }
            
            foreach (var existingLocation in existingEntry.HandleMap.Keys)
            {
                if (ResourceLocationEqualityComparer.Instance.Equals(existingLocation, resourceLocation))
                {
                    return true;
                }
            }
            
            return false;
        }

        protected void RegisterAssetToRegistry(
            IResourceLocation resourceLocation, 
            AsyncOperationHandle assetOpHandle, 
            bool isBatchLoaded,
            string primaryKey = null)
        {
            if (assetOpHandle.Status != AsyncOperationStatus.Succeeded)
            {
                return;
            }
            
            primaryKey ??= resourceLocation.PrimaryKey;
            
            if (_registry.TryGetAsset(primaryKey, out var existingEntry))
            {
                var locationExists = false;
                foreach (var existingLocation in existingEntry.HandleMap.Keys)
                {
                    if (!ResourceLocationEqualityComparer.Instance.Equals(existingLocation, resourceLocation))
                    {
                        continue;
                    }
                    
                    locationExists = true;
                    Verbose.D($"Location already exists for {primaryKey}. Skipping duplicate.");
                    break;
                }
                
                if (!locationExists)
                {
                    existingEntry.HandleMap[resourceLocation] = assetOpHandle;
                }
            }
            else
            {
                var assetEntry = new AssetEntry(primaryKey, isBatchLoaded)
                {
                    HandleMap =
                    {
                        [resourceLocation] = assetOpHandle
                    }
                };
                _registry.AddAsset(primaryKey, assetEntry);
            }
            
            HandleTrackingSystem.TrackHandle(primaryKey, assetOpHandle);
        }
        
        protected async UniTask MonitorLoadingProgress<T>(
            AsyncOperationHandle assetOpHandle, 
            ARMOperationHandle<T> operationHandle,
            float startProgress,
            float progressWeight)
        {
            var lastPercent = 0f;
            
            while (!assetOpHandle.IsDone)
            {
                if (assetOpHandle.PercentComplete > lastPercent)
                {
                    lastPercent = assetOpHandle.PercentComplete;
                    var progress = startProgress + (lastPercent * progressWeight);
                    operationHandle.Progress = Math.Min(progress, MAX_PROGRESS);
                }
                
                await UniTask.NextFrame();
            }
        }
    }
}
#endif