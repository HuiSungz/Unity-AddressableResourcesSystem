
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AddressableManage.Demo
{
    public class ActivatorWithAsync : MonoBehaviour
    {
        [SerializeField] private AssetReference _gameObjectRef;
        
        private async void Awake()
        {
            try
            {
                await ARM.ActivateAsync();
                
                Debug.Log("ARM System is ready");

                var opHandle = ARM.Assets.Load(_gameObjectRef);
                await opHandle.AsUniTask();
                // or
                // await ARM.Assets.Load(_gameObjectRef).AsUniTask();

                if (opHandle.HasError)
                {
                    Debug.LogError("Error: " + opHandle.OperationException);
                }
                else
                {
                    Debug.Log("Asset is loaded");
                    var assetEntry = opHandle.Result;
                    await assetEntry.InstantiateAsync();
                    Debug.Log("Instantiated");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}