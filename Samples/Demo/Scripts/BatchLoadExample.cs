
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace AddressableManage.Demo
{
    public class BatchLoadExample : MonoBehaviour
    {
        [SerializeField] private List<AssetLabelReference> _labelReferences;
        [SerializeField] private string _loadedAssetPrimaryKey;
        
        [Header("UI References")]
        [SerializeField] private Slider _progressSlider;
        
        private void Awake()
        {
            // Event callback event Register
            ARM.Callbacks.OnActivateCompleted += OnARMSystemActivateComplete;
            // Activate ARM System
            ARM.Activate();
        }

        private void OnDestroy()
        {
            // Event callback event Unregister
            ARM.Callbacks.OnActivateCompleted -= OnARMSystemActivateComplete;
        }

        private void OnARMSystemActivateComplete()
        {
            // Batch load
            // this is labels all individual load.
            // Can tracking the progress of the batch load.
            // Perhaps, you label is single, can 'new List<AssetLabelReference> { label }'
            var opHandle = ARM.Assets.BatchLoad(_labelReferences);
            opHandle.OnProgressChanged += (value) =>
            {
                _progressSlider.value = value;
            };

            // Batch load complete event
            opHandle.OnCompleted += (isSucceeded) =>
            {
                // Succeeded
                if (opHandle.Result)
                {
                    Debug.Log("Batch Load Succeeded");
                    if (ARM.Assets.TryGetLoadedEntry(_loadedAssetPrimaryKey, out var assetEntry))
                    {
                        assetEntry.Instantiate();
                        Debug.Log("Instantiated");
                    }
                    else
                    {
                        Debug.LogError("Asset Entry is null");
                    }

                    ARM.Assets.Load(_loadedAssetPrimaryKey).OnCompleted += (handle) =>
                    {
                        handle.Result.Instantiate();
                        Debug.Log("Instantiated Caching or None loaded");
                    };
                }
                // Fail
                else
                {
                    Debug.LogError("Batch Load Failed");
                }
            };
        }
    }
}