
#if ARM_UNITASK
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace ArchitectHS.AddressableManage
{
    /// <summary>
    /// Interface for scene loading and management operations
    /// </summary>
    public interface ISceneController
    {
        /// <summary>
        /// Loads a scene by key
        /// </summary>
        /// <param name="sceneKey">Scene key or address</param>
        /// <param name="loadMode">Load mode (Single or Additive)</param>
        /// <param name="activateOnLoad">Whether to activate the scene immediately after loading</param>
        /// <returns>Operation handle for the scene loading process</returns>
        ARMOperationHandle<SceneEntry> LoadScene(
            string sceneKey, 
            LoadSceneMode loadMode = LoadSceneMode.Additive, 
            bool activateOnLoad = true);
            
        /// <summary>
        /// Loads a scene by AssetReference
        /// </summary>
        /// <param name="sceneReference">Scene asset reference</param>
        /// <param name="loadMode">Load mode (Single or Additive)</param>
        /// <param name="activateOnLoad">Whether to activate the scene immediately after loading</param>
        /// <returns>Operation handle for the scene loading process</returns>
        ARMOperationHandle<SceneEntry> LoadScene(
            AssetReference sceneReference, 
            LoadSceneMode loadMode = LoadSceneMode.Additive, 
            bool activateOnLoad = true);
            
        /// <summary>
        /// Unloads a scene by key
        /// </summary>
        /// <param name="sceneKey">Scene key or address</param>
        /// <returns>Operation handle for the scene unloading process</returns>
        ARMOperationHandle<bool> UnloadScene(string sceneKey);
        
        /// <summary>
        /// Unloads a scene by AssetReference
        /// </summary>
        /// <param name="sceneReference">Scene asset reference</param>
        /// <returns>Operation handle for the scene unloading process</returns>
        ARMOperationHandle<bool> UnloadScene(AssetReference sceneReference);
        
        /// <summary>
        /// Unloads a scene entry
        /// </summary>
        /// <param name="sceneEntry">Scene entry to unload</param>
        /// <returns>Operation handle for the scene unloading process</returns>
        ARMOperationHandle<bool> UnloadScene(SceneEntry sceneEntry);
        
        /// <summary>
        /// Activates a previously loaded scene
        /// </summary>
        /// <param name="sceneEntry">Scene entry to activate</param>
        /// <returns>Operation handle for the scene activation process</returns>
        ARMOperationHandle<bool> ActivateScene(SceneEntry sceneEntry);
        
        /// <summary>
        /// Gets a loaded scene by key if it exists
        /// </summary>
        /// <param name="sceneKey">Scene key to find</param>
        /// <param name="sceneEntry">Output scene entry if found</param>
        /// <returns>True if scene was found, false otherwise</returns>
        bool TryGetLoadedEntry(string sceneKey, out SceneEntry sceneEntry);
        
        /// <summary>
        /// Gets a loaded scene by AssetReference if it exists
        /// </summary>
        /// <param name="sceneReference">Scene reference to find</param>
        /// <param name="sceneEntry">Output scene entry if found</param>
        /// <returns>True if scene was found, false otherwise</returns>
        bool TryGetLoadedEntry(AssetReference sceneReference, out SceneEntry sceneEntry);
        
        /// <summary>
        /// Unloads all scenes loaded through this controller
        /// </summary>
        /// <returns>Operation result</returns>
        UniTask<bool> UnloadAllScenes();
    }
}
#endif