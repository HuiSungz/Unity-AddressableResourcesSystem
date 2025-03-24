
#if ARM_UNITASK
using UnityEngine;

namespace AddressableManage
{
    public static class AssetEntryExtension
    {
        #region GameObject Instantiate Overloads

        /// <summary>
        /// Basic instantiation - Creates GameObject and adds tracker
        /// </summary>
        public static GameObject Instantiate(this AssetEntry assetEntry)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var instance = Object.Instantiate(prefab);
            var tracker = instance.AddComponent<MonoBehaviourTracker>();
            tracker.OnDestroyedCallback += assetEntry.Release;

            return instance;
        }

        /// <summary>
        /// Instantiation with position and rotation
        /// </summary>
        public static GameObject Instantiate(this AssetEntry assetEntry, Vector3 position, Quaternion rotation)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var instance = Object.Instantiate(prefab, position, rotation);
            var tracker = instance.AddComponent<MonoBehaviourTracker>();
            tracker.OnDestroyedCallback += assetEntry.Release;

            return instance;
        }

        /// <summary>
        /// Instantiation with position, rotation, and parent
        /// </summary>
        public static GameObject Instantiate(this AssetEntry assetEntry, Vector3 position, Quaternion rotation, Transform parent)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var instance = Object.Instantiate(prefab, position, rotation, parent);
            var tracker = instance.AddComponent<MonoBehaviourTracker>();
            tracker.OnDestroyedCallback += assetEntry.Release;

            return instance;
        }

        /// <summary>
        /// Instantiation with parent
        /// </summary>
        public static GameObject Instantiate(this AssetEntry assetEntry, Transform parent)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var instance = Object.Instantiate(prefab, parent);
            var tracker = instance.AddComponent<MonoBehaviourTracker>();
            tracker.OnDestroyedCallback += assetEntry.Release;

            return instance;
        }

        /// <summary>
        /// Instantiation with parent and world space option
        /// </summary>
        public static GameObject Instantiate(this AssetEntry assetEntry, Transform parent, bool instantiateInWorldSpace)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get 'GameObject' from assetEntry {assetEntry.Key}");
                return null;
            }

            var instance = Object.Instantiate(prefab, parent, instantiateInWorldSpace);
            var tracker = instance.AddComponent<MonoBehaviourTracker>();
            tracker.OnDestroyedCallback += assetEntry.Release;
            
            return instance;
        }

        #endregion

        #region Component Type Instantiation Overloads

        /// <summary>
        /// Basic component instantiation
        /// </summary>
        public static T Instantiate<T>(this AssetEntry assetEntry) where T : Component
        {
            var instance = assetEntry.Instantiate();
            if (!instance)
            {
                return null;
            }

            if (instance.TryGetComponent<T>(out var component))
            {
                return component;
            }
            
            Verbose.E($"Failed to get component of type {typeof(T).Name} from instantiated object");
            Object.Destroy(instance);
            return null;
        }

        /// <summary>
        /// Component instantiation with position and rotation
        /// </summary>
        public static T Instantiate<T>(this AssetEntry assetEntry, Vector3 position, Quaternion rotation) where T : Component
        {
            var instance = assetEntry.Instantiate(position, rotation);
            if (!instance)
            {
                return null;
            }

            if (instance.TryGetComponent<T>(out var component))
            {
                return component;
            }
            
            Verbose.E($"Failed to get component of type {typeof(T).Name} from instantiated object");
            Object.Destroy(instance);
            return null;
        }

        /// <summary>
        /// Component instantiation with position, rotation, and parent
        /// </summary>
        public static T Instantiate<T>(this AssetEntry assetEntry, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            var instance = assetEntry.Instantiate(position, rotation, parent);
            if (!instance)
            {
                return null;
            }

            if (instance.TryGetComponent<T>(out var component))
            {
                return component;
            }
            
            Verbose.E($"Failed to get component of type {typeof(T).Name} from instantiated object");
            Object.Destroy(instance);
            return null;
        }

        /// <summary>
        /// Component instantiation with parent
        /// </summary>
        public static T Instantiate<T>(this AssetEntry assetEntry, Transform parent) where T : Component
        {
            var instance = assetEntry.Instantiate(parent);
            if (!instance)
            {
                return null;
            }

            if (instance.TryGetComponent<T>(out var component))
            {
                return component;
            }
            
            Verbose.E($"Failed to get component of type {typeof(T).Name} from instantiated object");
            Object.Destroy(instance);
            return null;
        }

        /// <summary>
        /// Component instantiation with parent and world space option
        /// </summary>
        public static T Instantiate<T>(this AssetEntry assetEntry, Transform parent, bool instantiateInWorldSpace) where T : Component
        {
            var instance = assetEntry.Instantiate(parent, instantiateInWorldSpace);
            if (!instance)
            {
                return null;
            }

            if (instance.TryGetComponent<T>(out var component))
            {
                return component;
            }
            
            Verbose.E($"Failed to get component of type {typeof(T).Name} from instantiated object");
            Object.Destroy(instance);
            return null;
        }

        #endregion

        #region Instantiate Without Tracker Overloads

        /// <summary>
        /// Basic instantiation without tracker
        /// </summary>
        public static GameObject InstantiateWithoutTracker(this AssetEntry assetEntry)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get GameObject from AssetEntry {assetEntry.Key}");
                return null;
            }
            
            return Object.Instantiate(prefab);
        }

        /// <summary>
        /// Instantiation without tracker - with position and rotation
        /// </summary>
        public static GameObject InstantiateWithoutTracker(this AssetEntry assetEntry, Vector3 position, Quaternion rotation)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get GameObject from AssetEntry {assetEntry.Key}");
                return null;
            }
            
            return Object.Instantiate(prefab, position, rotation);
        }

        /// <summary>
        /// Instantiation without tracker - with position, rotation, and parent
        /// </summary>
        public static GameObject InstantiateWithoutTracker(this AssetEntry assetEntry, Vector3 position, Quaternion rotation, Transform parent)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get GameObject from AssetEntry {assetEntry.Key}");
                return null;
            }
            
            return Object.Instantiate(prefab, position, rotation, parent);
        }

        /// <summary>
        /// Instantiation without tracker - with parent
        /// </summary>
        public static GameObject InstantiateWithoutTracker(this AssetEntry assetEntry, Transform parent)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get GameObject from AssetEntry {assetEntry.Key}");
                return null;
            }
            
            return Object.Instantiate(prefab, parent);
        }

        /// <summary>
        /// Instantiation without tracker - with parent and world space option
        /// </summary>
        public static GameObject InstantiateWithoutTracker(this AssetEntry assetEntry, Transform parent, bool instantiateInWorldSpace)
        {
            var prefab = assetEntry.Get<GameObject>();
            if (!prefab)
            {
                Verbose.E($"Failed to get GameObject from AssetEntry {assetEntry.Key}");
                return null;
            }
            
            return Object.Instantiate(prefab, parent, instantiateInWorldSpace);
        }

        #endregion
    }
}
#endif