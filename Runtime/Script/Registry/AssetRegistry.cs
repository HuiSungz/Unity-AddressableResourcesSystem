
#if ARM_UNITASK
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ArchitectHS.AddressableManage
{
    internal class AssetRegistry : IAssetRegistry
    {
        #region Fields

        private readonly Dictionary<AssetLabelReference, IList<IResourceLocation>> _labelLocations = new();
        private readonly Dictionary<string, AssetEntry> _assets = new();
        private readonly Dictionary<string, SceneEntry> _scenes = new();
        private readonly List<IResourceLocation> _noLocations = new(0);
        private readonly HashSet<object> _keys = new();
        
        public Dictionary<AssetLabelReference, IList<IResourceLocation>> LabelLocations => _labelLocations;
        public Dictionary<string, AssetEntry> Assets => _assets;
        public Dictionary<string, SceneEntry> Scenes => _scenes;
        public HashSet<object> Keys => _keys;
        public List<IResourceLocation> NoLocations => _noLocations;

        #endregion
        
        public void Clear()
        {
            _labelLocations.Clear();
            _assets.Clear();
            _scenes.Clear();
            _keys.Clear();
        }

        public void AddKey(object key)
        {
            if (key != null)
            {
                _keys.Add(key);
            }
        }

        public bool TryGetAsset(string key, out AssetEntry entry)
        {
            return _assets.TryGetValue(key, out entry);
        }

        public void AddAsset(string key, AssetEntry entry)
        {
            _assets[key] = entry;
        }

        public void RemoveAsset(string key)
        {
            _assets.Remove(key);
        }
    }
}
#endif