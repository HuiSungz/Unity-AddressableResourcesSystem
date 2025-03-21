
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace AddressableManage
{
    internal static class TypedAssetLoader
    {
        private static readonly Dictionary<Type, Func<IResourceLocation, AsyncOperationHandle>> Loaders = 
            new()
            {
                { typeof(Sprite), location => Addressables.LoadAssetAsync<Sprite>(location) },
                { typeof(Texture2D), location => Addressables.LoadAssetAsync<Texture2D>(location) },
                { typeof(Material), location => Addressables.LoadAssetAsync<Material>(location) },
                { typeof(AudioClip), location => Addressables.LoadAssetAsync<AudioClip>(location) },
                { typeof(GameObject), location => Addressables.LoadAssetAsync<GameObject>(location) },
                { typeof(TextAsset), location => Addressables.LoadAssetAsync<TextAsset>(location) },
                { typeof(Mesh), location => Addressables.LoadAssetAsync<Mesh>(location) },
                { typeof(AnimationClip), location => Addressables.LoadAssetAsync<AnimationClip>(location) },
                { typeof(Font), location => Addressables.LoadAssetAsync<Font>(location) },
                { typeof(ScriptableObject), location => Addressables.LoadAssetAsync<ScriptableObject>(location) },
                { typeof(Shader), location => Addressables.LoadAssetAsync<Shader>(location) },
                { typeof(VideoClip), location => Addressables.LoadAssetAsync<VideoClip>(location) },
                { typeof(ComputeShader), location => Addressables.LoadAssetAsync<ComputeShader>(location) },
                { typeof(RuntimeAnimatorController), location => Addressables.LoadAssetAsync<RuntimeAnimatorController>(location) }
            };
        
        public static AsyncOperationHandle LoadAssetByType(IResourceLocation location)
        {
            var resourceType = location.ResourceType;
            
            if (resourceType == null)
            {
                Verbose.W($"Unable to determine type for resource location: {location.PrimaryKey}, using default Object type");
                return Addressables.LoadAssetAsync<Object>(location);
            }
            
            try
            {
                if (Loaders.TryGetValue(resourceType, out var loaderFunc))
                {
                    return loaderFunc(location);
                }
                
                foreach (var entry in Loaders.Where(entry => resourceType.IsSubclassOf(entry.Key)))
                {
                    return entry.Value(location);
                }
                
                Verbose.D($"Unhandled resource type: {resourceType.Name}, using default Object type for {location.PrimaryKey}");
                return Addressables.LoadAssetAsync<Object>(location);
            }
            catch (Exception exception)
            {
                Verbose.Ex($"Error loading asset of type {resourceType.Name} for {location.PrimaryKey}", exception);
                return Addressables.LoadAssetAsync<Object>(location);
            }
        }
    }
}