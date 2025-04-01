
#if ARM_UNITASK
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace ArchitectHS.AddressableManage
{
    internal interface IAssetRegistry
    {
        Dictionary<AssetLabelReference, IList<IResourceLocation>> LabelLocations { get; }
        Dictionary<string, AssetEntry> Assets { get; }
        Dictionary<string, SceneEntry> Scenes { get; }
        HashSet<object> Keys { get; }
        List<IResourceLocation> NoLocations { get; }

        void Clear();
        void AddKey(object key);
        
        bool TryGetAsset(string key, out AssetEntry entry);
        void AddAsset(string key, AssetEntry entry);
        void RemoveAsset(string key);
    }
}
#endif