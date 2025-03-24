
#if ARM_UNITASK
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace AddressableManage
{
    public interface IAssetController
    {
        /// <summary>
        /// Performs batch loading of assets.
        /// </summary>
        /// <param name="labelReferences">Labels to batch load</param>
        /// <returns>Asynchronous operation handle</returns>
        ARMOperationHandle<bool> BatchLoad(List<AssetLabelReference> labelReferences);
        
        /// <summary>
        /// Loads an individual asset using a string key.
        /// </summary>
        /// <param name="key">Asset key to load</param>
        /// <returns>Asynchronous operation handle</returns>
        ARMOperationHandle<AssetEntry> Load(string key);
        
        /// <summary>
        /// Loads an individual asset using an AssetReference.
        /// </summary>
        /// <param name="assetReference">Asset reference to load</param>
        /// <returns>Asynchronous operation handle</returns>
        ARMOperationHandle<AssetEntry> Load(AssetReference assetReference);
        
        /// <summary>
        /// Releases batch-loaded assets corresponding to the specified label references.
        /// </summary>
        /// <param name="labelReferences">List of label references to release</param>
        void ReleaseBatches(List<AssetLabelReference> labelReferences);
        
        /// <summary>
        /// Releases all batch-loaded assets.
        /// </summary>
        void ReleaseAllBatches();
        
        /// <summary>
        /// Synchronously retrieves an already loaded asset. Does not load assets that aren't already loaded.
        /// </summary>
        /// <param name="key">Asset key to find</param>
        /// <param name="assetEntry">Found asset entry (null if not found)</param>
        /// <returns>True if the asset was found, false otherwise</returns>
        bool TryGetLoadedEntry(string key, out AssetEntry assetEntry);
        
        /// <summary>
        /// Synchronously retrieves an already loaded asset. Does not load assets that aren't already loaded.
        /// </summary>
        /// <param name="assetReference">Asset reference to find</param>
        /// <param name="assetEntry">Found asset entry (null if not found)</param>
        /// <returns>True if the asset was found, false otherwise</returns>
        bool TryGetLoadedEntry(AssetReference assetReference, out AssetEntry assetEntry);
    }
}
#endif