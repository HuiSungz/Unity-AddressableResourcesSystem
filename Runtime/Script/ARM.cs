
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableManage
{
    public class ARM
    {
        #region Static Interface
        
        private static ARM _instance;
        private static ARM Instance => _instance ??= new ARM();
        
        public static IAssetController Assets => Instance._assetController;
        public static ISceneController Scenes => Instance._sceneController;
        public static bool Initialized { get; private set; }
        
        public static void Activate(bool autoReleaseHandle = true)
        {
            Instance.ActivateInternal(autoReleaseHandle);
        }
        
        public static async UniTask<IResourceLocator> ActivateAsync(bool autoReleaseHandle = true)
        {
            return await Instance.ActivateAsyncInternal(autoReleaseHandle);
        }
        
        internal static void ReleaseAsset(string key)
        {
            (Instance._assetController as AssetController)?.ReleaseAsset(key);
        }

        #endregion
        
        #region Instance Implementation
        
        private readonly IAssetRegistry _registry;
        private readonly IAssetController _assetController;
        private readonly ISceneController _sceneController;
        
        private ARM()
        {
            _registry = new AssetRegistry();

            var container = DependencyContainer.Instance;
            container.Register(_registry);
            
            _assetController = new AssetController();
            _sceneController = new SceneController();
        }

        private void ActivateInternal(bool autoReleaseHandle)
        {
            _registry.Clear();
            HandleTrackingSystem.ReleaseAllHandles();
            
            try
            {
                var operationHandle = Addressables.InitializeAsync();
                operationHandle.Completed += handle =>
                {
                    OnInitializeCompleted(handle);

                    if (autoReleaseHandle)
                    {
                        handle.Release();
                    }
                };
            }
            catch (Exception exception)
            {
                Verbose.Ex("ARM.Activate activate error", exception);
            }
        }

        private async UniTask<IResourceLocator> ActivateAsyncInternal(bool autoReleaseHandle)
        {
            _registry.Clear();
            HandleTrackingSystem.ReleaseAllHandles();
            
            try
            {
                var operationHandle = Addressables.InitializeAsync(false);
                var resourceLocator = await operationHandle.ToUniTask();

                OnInitializeCompleted(operationHandle);

                if (autoReleaseHandle)
                {
                    operationHandle.Release();
                }
                
                return resourceLocator;
            }
            catch (Exception exception)
            {
                Verbose.Ex("ARM.ActivateAsync activate error", exception);
                return null;
            }
        }
        
        private void OnInitializeCompleted(AsyncOperationHandle<IResourceLocator> operationHandle)
        {
            if (operationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Verbose.E("Failed to initialize Addressable System.");
                return;
            }

            foreach (var key in operationHandle.Result.Keys)
            {
                _registry.AddKey(key);
            }
            
            Initialized = true;
        }
        
        #endregion
    }
}