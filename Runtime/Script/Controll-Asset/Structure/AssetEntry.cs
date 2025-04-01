
#if ARM_UNITASK
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace ArchitectHS.AddressableManage
{
    public class AssetEntry
    {
        #region Fields
        
        internal ushort ReferenceCount;
        internal Dictionary<IResourceLocation, AsyncOperationHandle> HandleMap;

        public readonly string Key;
        public readonly bool IsBatchLoaded;
        
        #endregion
        
        internal AssetEntry(string key, bool isBatchLoaded)
        {
            Key = key;
            IsBatchLoaded = isBatchLoaded;
            ReferenceCount = 0;
            HandleMap = new Dictionary<IResourceLocation, AsyncOperationHandle>(ResourceLocationEqualityComparer.Instance);
        }

        #region Get and Release

        public T Get<T>() where T : Object
        {
            foreach (var handle in HandleMap.Values)
            {
                if (handle.Result is not T dependencyResult)
                {
                    continue;
                }
                
                ++ReferenceCount;
                return dependencyResult;
            }

            Verbose.W($"Failed to get asset of type {typeof(T).Name} for key {Key}");
            return null;
        }

        public void Release()
        {
            if (IsBatchLoaded)
            {
                Verbose.W($"Batch loaded asset {Key} cannot be released.");
                return;
            }
            
            if (ReferenceCount <= 0)
            {
                Verbose.W($"Attempting to release asset {Key} with reference count already at {ReferenceCount}");
                return;
            }

            if (--ReferenceCount > 0)
            {
                Verbose.D($"Reference count decreased to {ReferenceCount} for asset {Key}");
                return;
            }
            
            ARM.ReleaseAsset(Key);
        }

        #endregion
    }
}
#endif