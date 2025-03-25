
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AddressableManage.Demo
{
    public class ActivatorWithSync : MonoBehaviour
    {
        [SerializeField] private AssetReference _gameObjectRef;
        
        private void Awake()
        {
            ARM.Callbacks.OnActivateCompleted += () =>
            {
                Debug.Log("Activate Completed.");
                if (_gameObjectRef == null)
                {
                    Debug.LogWarning("Asset Reference is null. setup the AssetReference.");
                }
                else
                {
                    ARM.Assets.Load(_gameObjectRef).OnCompleted += (assetEntryHandle) =>
                    {
                        if (assetEntryHandle.HasError)
                        {
                            Debug.LogError("Asset OperationHandle is error");
                        }
                        else
                        {
                            var assetEntry = assetEntryHandle.Result;
                            assetEntry.Instantiate();
                            Debug.Log($"Instantiate Completed. {assetEntry.Key}");
                        }
                    };
                }
            };

            ARM.Activate();
            
            // auto release handle is false
            // ARM.Activate(false);
        }
    }
}