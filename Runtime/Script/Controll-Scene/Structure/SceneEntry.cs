
#if ARM_UNITASK
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ArchitectHS.AddressableManage
{
    /// <summary>
    /// Represents a loaded scene with its handle and state information
    /// </summary>
    public class SceneEntry
    {
        /// <summary>
        /// The key used to identify this scene
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// The scene loading mode used (Single or Additive)
        /// </summary>
        public LoadSceneMode SceneMode { get; }
        
        /// <summary>
        /// The handle to the loaded scene
        /// </summary>
        public AsyncOperationHandle<SceneInstance> SceneHandle { get; internal set; }
        
        /// <summary>
        /// The scene instance result
        /// </summary>
        public SceneInstance SceneInstance => SceneHandle.Result;
        
        /// <summary>
        /// The Unity scene object
        /// </summary>
        public Scene Scene => SceneInstance.Scene;
        
        /// <summary>
        /// Whether the scene handle is valid and succeeded
        /// </summary>
        public bool IsValid => SceneHandle.IsValid() && SceneHandle.Status == AsyncOperationStatus.Succeeded;

        /// <summary>
        /// Creates a new scene entry
        /// </summary>
        internal SceneEntry(string key, LoadSceneMode sceneMode)
        {
            Key = key;
            SceneMode = sceneMode;
        }
        
        /// <summary>
        /// Activates this scene if it wasn't activated on load
        /// </summary>
        public ARMOperationHandle<bool> Activate()
        {
            return ARM.Scenes.ActivateScene(this);
        }
    }
}
#endif