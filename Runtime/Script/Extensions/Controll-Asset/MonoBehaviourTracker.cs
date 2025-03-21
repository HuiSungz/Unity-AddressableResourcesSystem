
using System;
using UnityEngine;

namespace AddressableManage
{
    internal class MonoBehaviourTracker : MonoBehaviour
    {
        #region Fields
        
        public event Action OnDestroyedCallback;
        
        #endregion
        
        private void OnDestroy()
        {
            OnDestroyedCallback?.Invoke();
            OnDestroyedCallback = null;
        }
    }
}