
#if ARM_UNITASK
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ArchitectHS.AddressableManage
{
    internal class AssetBatchLoader : AssetLoaderBase
    {
        #region Fields
        
        private int _totalAssetCount;
        private int _loadedAssetCount;
        private int _assetsToLoad;
        private int _alreadyLoadedCount;
        private float _currentAssetProgress;
        
        #endregion
        
        public async UniTask<bool> LoadAsync(
            List<AssetLabelReference> labelReferences,
            ARMOperationHandle<bool> operationHandle)
        {
            try
            {
                InitializeLoadingState(operationHandle);
                
                var labelLocations = await GetLabelLocations(labelReferences);
                if (labelLocations.Count == 0)
                {
                    Verbose.E("BatchLoader: No locations found for labels");
                    return false;
                }
                
                await ProcessLocations(labelLocations, operationHandle);
                
                operationHandle.Progress = 1.0f;
                return true;
            }
            catch (Exception exception)
            {
                Verbose.Ex("BatchLoader failed to load assets.", exception);
                return false;
            }
        }

        #region Initialization

        private void InitializeLoadingState(ARMOperationHandle<bool> operationHandle)
        {
            _loadedAssetCount = 0;
            _currentAssetProgress = MIN_PROGRESS;
            operationHandle.Progress = MIN_PROGRESS;
        }
        
        private async UniTask<IList<IResourceLocation>> GetLabelLocations(List<AssetLabelReference> labelReferences)
        {
            try
            {
                var resultLocation = new List<IResourceLocation>();
                
                foreach (var labelReference in labelReferences)
                {
                    if (_registry.LabelLocations.TryGetValue(labelReference, out var locations))
                    {
                        resultLocation.AddRange(locations);
                        continue;
                    }
                    
                    var needLocation = await LoadLocationAsync(labelReference);
                    if (needLocation.Count == 0)
                    {
                        Verbose.W($"Label {labelReference} not found or is empty");
                        continue;
                    }
                    
                    _registry.LabelLocations[labelReference] = needLocation;
                    resultLocation.AddRange(needLocation);
                }
                
                return resultLocation.Count != 0 ? resultLocation : _registry.NoLocations;
            }
            catch (Exception exception)
            {
                Verbose.Ex("BatchLoader load label location Failed.", exception);
                return _registry.NoLocations;
            }
        }

        #endregion
        
        private async UniTask ProcessLocations(IList<IResourceLocation> labelLocations, ARMOperationHandle<bool> operationHandle)
        {
            InitializeAssetCounts(labelLocations);
            
            foreach (var location in labelLocations)
            {
                await ProcessLocation(location, operationHandle);
            }
        }
        
        private void InitializeAssetCounts(IList<IResourceLocation> labelLocations)
        {
            _totalAssetCount = labelLocations.Count;
            _alreadyLoadedCount = CountAlreadyLoadedAssets(labelLocations);
            _assetsToLoad = _totalAssetCount - _alreadyLoadedCount;

            if (_alreadyLoadedCount <= 0)
            {
                return;
            }
            
            _loadedAssetCount = _alreadyLoadedCount;
        }
        
        private int CountAlreadyLoadedAssets(IList<IResourceLocation> labelLocations)
        {
            var count = 0;
            foreach (var location in labelLocations)
            {
                if (IsAssetAlreadyLoaded(location))
                {
                    count++;
                }
            }
            return count;
        }

        #region Progress

        private void UpdateProgress(ARMOperationHandle<bool> operationHandle, float value)
        {
            operationHandle.Progress = value;
        }
        
        private void UpdateBatchProgress(ARMOperationHandle<bool> operationHandle)
        {
            var completedAssetsProgress = (float)_loadedAssetCount / _totalAssetCount;
            var currentAssetContribution = CalculateCurrentAssetContribution();
            
            var totalProgress = completedAssetsProgress + currentAssetContribution;
            totalProgress = Math.Min(totalProgress, MAX_PROGRESS);
            
            UpdateProgress(operationHandle, totalProgress);
        }
        
        private float CalculateCurrentAssetContribution()
        {
            if (_assetsToLoad <= 0 || _loadedAssetCount >= _totalAssetCount)
            {
                return 0f;
            }
            
            return _currentAssetProgress / _totalAssetCount;
        }

        #endregion
        
        private async UniTask ProcessLocation(IResourceLocation resourceLocation, ARMOperationHandle<bool> operationHandle)
        {
            try
            {
                if (IsAssetAlreadyLoaded(resourceLocation))
                {
                    Verbose.D($"BatchLoader: Asset already loaded. Skipping. KEY {resourceLocation.PrimaryKey}");
                    return;
                }
                
                await LoadAssetForLocation(resourceLocation, operationHandle);
                
                IncrementLoadedAssetCount(operationHandle);
            }
            catch (Exception exception)
            {
                Verbose.Ex($"BatchLoader ProcessLocation Failed for {resourceLocation.PrimaryKey}.", exception);
                IncrementLoadedAssetCount(operationHandle);
            }
        }
        
        private async UniTask LoadAssetForLocation(IResourceLocation resourceLocation, ARMOperationHandle<bool> operationHandle)
        {
            var asyncOperationHandle = await LoadAssetWithProgressTracking(resourceLocation, operationHandle);
            
            RegisterAssetToRegistry(resourceLocation, asyncOperationHandle, true);
        }
        
        private void IncrementLoadedAssetCount(ARMOperationHandle<bool> operationHandle)
        {
            _loadedAssetCount++;
            _currentAssetProgress = MIN_PROGRESS;
            UpdateBatchProgress(operationHandle);
        }

        private async UniTask<AsyncOperationHandle> LoadAssetWithProgressTracking(
            IResourceLocation resourceLocation, ARMOperationHandle<bool> operationHandle)
        {
            AsyncOperationHandle assetOpHandle = default;
            
            try
            {
                assetOpHandle = TypedAssetLoader.LoadAssetByType(resourceLocation);

                await MonitorLoadingProgress(assetOpHandle, operationHandle, operationHandle.Progress, 1f / _totalAssetCount);
                await assetOpHandle.ToUniTask();
                return assetOpHandle;
            }
            catch (Exception exception)
            {
                Verbose.Ex("BatchLoader LoadAssetWithProgressTracking Failed.", exception);
                
                if (assetOpHandle.IsValid())
                {
                    assetOpHandle.Release();
                }
                
                return default;
            }
        }
    }
}
#endif