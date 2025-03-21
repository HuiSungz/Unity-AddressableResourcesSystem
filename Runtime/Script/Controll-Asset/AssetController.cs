
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableManage
{
    internal class AssetController : IAssetController
    {
        #region Fields

        [DependencyInject] private IAssetRegistry _registry;

        private readonly AssetBatchLoader _batchLoader;
        private readonly AssetIndividualLoader _individualLoader;

        #endregion

        public AssetController()
        {
            DependencyContainer.Instance.InjectDependencies(this);
            
            _batchLoader = new AssetBatchLoader();
            _individualLoader = new AssetIndividualLoader();
        }

        #region LOAD - BATCH

        public ARMOperationHandle<bool> BatchLoad(List<AssetLabelReference> labelReferences)
        {
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. you should call ARM.Initialize() first.");
                return new ARMOperationHandle<bool>();
            }
            
            var armOperationHandle = new ARMOperationHandle<bool>();

            _ = LoadBatchAsync(labelReferences, armOperationHandle);
            
            return armOperationHandle;
        }

        private async UniTask LoadBatchAsync(List<AssetLabelReference> labelReferences, ARMOperationHandle<bool> armOperationHandle)
        {
            try
            {
                var success = await _batchLoader.LoadAsync(labelReferences, armOperationHandle);
                if (success)
                {
                    armOperationHandle.Complete(true);
                }
                else
                {
                    armOperationHandle.Fail(new Exception("Failed to load assets"));
                }
            }
            catch (Exception exception)
            {
                armOperationHandle.Fail(exception);
            }
        }

        #endregion

        #region LOAD - INDIVIDUAL

        public ARMOperationHandle<AssetEntry> Load(string key)
        {
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. you should call ARM.Initialize() first.");
                return new ARMOperationHandle<AssetEntry>();
            }
            
            var armOperationHandle = new ARMOperationHandle<AssetEntry>();
            
            _ = _individualLoader.LoadAsync(key, armOperationHandle);
            
            return armOperationHandle;
        }
        
        public ARMOperationHandle<AssetEntry> Load(AssetReference assetReference)
        {
            if (!ARM.Initialized)
            {
                Verbose.W("Not initialized. you should call ARM.Initialize() first.");
                return new ARMOperationHandle<AssetEntry>();
            }
            
            var armOperationHandle = new ARMOperationHandle<AssetEntry>();
            
            _ = _individualLoader.LoadAsync(assetReference, armOperationHandle);
            
            return armOperationHandle;
        }

        #endregion
        
        #region GET BY LOCATION

        public bool TryGetLoadedEntry(string key, out AssetEntry assetEntry)
        {
            assetEntry = null;
            
            try
            {
                var locationHandle = Addressables.LoadResourceLocationsAsync(key);
                locationHandle.WaitForCompletion();
                
                if (locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result.Count == 0)
                {
                    locationHandle.Release();
                    return false;
                }

                var primaryKey = locationHandle.Result[0].PrimaryKey;
                locationHandle.Release();
                
                return _registry.TryGetAsset(primaryKey, out assetEntry);
            }
            catch (Exception exception)
            {
                Verbose.Ex("TryGetLoadedEntryByLocationKey 오류", exception);
                return false;
            }
        }

        public bool TryGetLoadedEntry(AssetReference assetReference, out AssetEntry assetEntry)
        {
            assetEntry = null;
            if (assetReference == null)
            {
                return false;
            }
            
            try
            {
                var locationHandle = Addressables.LoadResourceLocationsAsync(assetReference);
                locationHandle.WaitForCompletion();
                
                if (locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result.Count == 0)
                {
                    locationHandle.Release();
                    return false;
                }
                
                var primaryKey = locationHandle.Result[0].PrimaryKey;
                locationHandle.Release();
                
                return _registry.TryGetAsset(primaryKey, out assetEntry);
            }
            catch (Exception exception)
            {
                Verbose.Ex("TryGetLoadedEntryByLocationKey 오류", exception);
                return false;
            }
        }

        #endregion

        #region Release

        internal void ReleaseAsset(string key)
        {
            if (!_registry.TryGetAsset(key, out var entry) || entry.IsBatchLoaded)
            {
                return;
            }

            HandleTrackingSystem.ReleaseHandles(key);
            
            _registry.RemoveAsset(key);
        }
        
        public void ReleaseBatches(List<AssetLabelReference> labelReferences)
        {
            if (labelReferences == null || labelReferences.Count == 0)
            {
                return;
            }
            
            var assetKeysToRelease = new HashSet<string>();
            foreach (var labelRef in labelReferences)
            {
                if (!_registry.LabelLocations.TryGetValue(labelRef, out var locations))
                {
                    continue;
                }
                
                foreach (var location in locations)
                {
                    assetKeysToRelease.Add(location.PrimaryKey);
                }

                _registry.LabelLocations.Remove(labelRef);
            }
            
            foreach (var assetKey in assetKeysToRelease)
            {
                if (!_registry.TryGetAsset(assetKey, out var entry) || !entry.IsBatchLoaded)
                {
                    continue;
                }
                
                HandleTrackingSystem.ReleaseHandles(assetKey);
                
                _registry.RemoveAsset(assetKey);
            }
        }
        
        public void ReleaseAllBatches()
        {
            var batchLoadedAssets = _registry.Assets
                .Where(assetPair => assetPair.Value.IsBatchLoaded)
                .Select(assetPair => assetPair.Key)
                .ToList();
                
            foreach (var assetKey in batchLoadedAssets)
            {
                HandleTrackingSystem.ReleaseHandles(assetKey);
                
                _registry.RemoveAsset(assetKey);
            }
            
            _registry.LabelLocations.Clear();
        }

        #endregion
    }
}