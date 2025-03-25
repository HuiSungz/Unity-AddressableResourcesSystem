
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AddressableManage.Demo
{
    public class ReleaseExample : MonoBehaviour
    {
        [SerializeField] private string _loadedAssetSpriteKey;

        [Header("UI References")] 
        [SerializeField] private Image _image;

        // Caching to prevent garbage collection
        // Don't use eventmethod or Dispose => Release();
        // Release with Get<T> Counted.
        private AssetEntry _loadedEntry;
        
        private async void Start()
        {
            try
            {
                await ARM.ActivateAsync();
                
                Debug.Log("ARM System is ready");

                var opHandle = ARM.Assets.Load(_loadedAssetSpriteKey);
                await opHandle.AsUniTask();

                if (opHandle.HasError)
                {
                    Debug.LogError("Error: " + opHandle.OperationException);
                }
                else
                {
                    Debug.Log("Asset is loaded");
                    _loadedEntry = opHandle.Result;
                    _image.sprite = _loadedEntry.Get<Sprite>();
                    _image.sprite = _loadedEntry.Get<Sprite>();
                    Debug.Log("You use the reference count is 2");

                    await UniTask.Delay(3000);

                    Destroy(gameObject);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Error: " + exception);
            }
        }

        private void OnDestroy()
        {
            // Can't release (reference count is 2-1 = 1)
            // _loadedEntry.Release();
            
            // Can release (reference count is 2-2 = 0)
            // Safety release, 'image.sprite' reference sprite is null.
            _image.sprite = null;
            _loadedEntry.Release();
            _loadedEntry.Release();
        }
    }
}