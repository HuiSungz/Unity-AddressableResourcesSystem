
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ArchitectHS.AddressableManage
{
    internal static class HandleTrackingSystem
    {
        private static readonly Dictionary<string, List<AsyncOperationHandle>> TrackedHandles = new();
        
        public static void TrackHandle(string key, AsyncOperationHandle handle)
        {
            if (!handle.IsValid())
            {
                return;
            }
            
            if (!TrackedHandles.TryGetValue(key, out var handles))
            {
                handles = new List<AsyncOperationHandle>();
                TrackedHandles[key] = handles;
            }
            
            handles.Add(handle);
        }
        
        public static void ReleaseHandles(string key)
        {
            if (!TrackedHandles.TryGetValue(key, out var handles))
            {
                return;
            }
            
            foreach (var handle in handles.Where(handle => handle.IsValid()))
            {
                try
                {
                    handle.Release();
                }
                catch (Exception ex)
                {
                    Verbose.Ex($"Error releasing handle for {key}", ex);
                }
            }
            
            TrackedHandles.Remove(key);
        }
        
        public static void ReleaseAllHandles()
        {
            foreach (var keyHandlesPair in TrackedHandles)
            {
                foreach (var handle in keyHandlesPair.Value.Where(handle => handle.IsValid()))
                {
                    try
                    {
                        handle.Release();
                    }
                    catch (Exception ex)
                    {
                        Verbose.Ex($"Error releasing handle for {keyHandlesPair.Key}", ex);
                    }
                }
            }
            
            TrackedHandles.Clear();
        }
    }
}