
#if ARM_UNITASK
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ArchitectHS.AddressableManage
{
    public static class AssetEntryAsyncExtension
    {
        /// <summary>
        /// Basic async instantiation
        /// </summary>
        public static AsyncInstantiateOperation InstantiateAsync(this AssetEntry assetEntry)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var handle = Object.InstantiateAsync(prefab);
            _ = SetupTrackerAsync(handle, assetEntry);
            return handle;
        }

        /// <summary>
        /// Async instantiation with position and rotation
        /// </summary>
        public static AsyncInstantiateOperation InstantiateAsync(this AssetEntry assetEntry, Vector3 position, Quaternion rotation)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var handle = Object.InstantiateAsync(prefab, position, rotation);
            _ = SetupTrackerAsync(handle, assetEntry);
            return handle;
        }

        /// <summary>
        /// Async instantiation with position, rotation, and parent
        /// </summary>
        public static AsyncInstantiateOperation InstantiateAsync(this AssetEntry assetEntry, Vector3 position, Quaternion rotation, Transform parent)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var parameters = new InstantiateParameters
            {
                parent = parent
            };
            var handle = Object.InstantiateAsync(prefab, position, rotation, parameters);
            _ = SetupTrackerAsync(handle, assetEntry);
            return handle;
        }

        /// <summary>
        /// Async instantiation with parent
        /// </summary>
        public static AsyncInstantiateOperation InstantiateAsync(this AssetEntry assetEntry, Transform parent)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var parameters = new InstantiateParameters
            {
                parent = parent
            };
            var handle = Object.InstantiateAsync(prefab, parameters);
            _ = SetupTrackerAsync(handle, assetEntry);
            return handle;
        }

        /// <summary>
        /// Async instantiation with parent and world space option
        /// </summary>
        public static AsyncInstantiateOperation InstantiateAsync(this AssetEntry assetEntry, Transform parent, bool worldPositionStays)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var parameters = new InstantiateParameters
            {
                parent = parent,
                worldSpace = worldPositionStays
            };
            var handle = Object.InstantiateAsync(prefab, parameters);
            _ = SetupTrackerAsync(handle, assetEntry);
            return handle;
        }

        private static async UniTask SetupTrackerAsync(AsyncInstantiateOperation handle, AssetEntry assetEntry)
        {
            if (handle == null)
            {
                return;
            }

            handle.allowSceneActivation = true;
            
            while (!handle.isDone)
            {
                await UniTask.Yield();
            }

            var instance = handle.Result[0] as GameObject;
            if (instance)
            {
                var tracker = instance.AddComponent<MonoBehaviourTracker>();
                tracker.OnDestroyedCallback += assetEntry.Release;
            }
        }
    }
}
#endif