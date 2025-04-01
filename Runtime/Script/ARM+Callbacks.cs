
using System;

namespace ArchitectHS.AddressableManage
{
    public sealed partial class ARM
    {
        #region Access - Static class

        public static class Callbacks
        {
            #region Events

            public static event Action OnActivateCompleted;
            public static event Action OnActivateFailed;

            #endregion

            #region Raised

            internal static void RaiseOnActivateCompleted()
            {
                OnActivateCompleted?.Invoke();
            }

            internal static void RaiseOnActivateFailed()
            {
                OnActivateFailed?.Invoke();
            }

            #endregion
        }

        #endregion
    }
}