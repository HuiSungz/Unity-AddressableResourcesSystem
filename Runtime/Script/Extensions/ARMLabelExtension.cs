
#if ARM_UNITASK
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace AddressableManage
{
    /// <summary>
    /// Extension methods for label management in the ARM system
    /// </summary>
    public static class ARMLabelExtensions
    {
        /// <summary>
        /// Gets the internal registry from ARM instance
        /// </summary>
        private static IAssetRegistry GetRegistry(this ARM arm)
        {
            // In a real-world scenario, you would add an internal property to ARM 
            // instead of using reflection
            return ARM.GetInternalRegistry();
        }
        
        /// <summary>
        /// Returns labels that need to be loaded (excluding already loaded labels)
        /// </summary>
        /// <param name="arm">ARM instance</param>
        /// <param name="newLabels">New labels that need to be loaded</param>
        /// <returns>List of labels that actually need to be loaded</returns>
        public static List<AssetLabelReference> GetLabelsToLoad(this ARM arm, List<AssetLabelReference> newLabels)
        {
            if (newLabels == null || newLabels.Count == 0)
            {
                return new List<AssetLabelReference>();
            }
            
            var registry = arm.GetRegistry();
            return newLabels.Where(label => !registry.LabelLocations.ContainsKey(label)).ToList();
        }
        
        /// <summary>
        /// Returns labels that need to be unloaded (no longer needed)
        /// </summary>
        /// <param name="arm">ARM instance</param>
        /// <param name="requiredLabels">Labels that are still required</param>
        /// <returns>List of labels that should be unloaded</returns>
        public static List<AssetLabelReference> GetLabelsToUnload(this ARM arm, List<AssetLabelReference> requiredLabels)
        {
            var registry = arm.GetRegistry();
            var loadedLabels = new List<AssetLabelReference>(registry.LabelLocations.Keys);
            
            if (requiredLabels == null || requiredLabels.Count == 0)
            {
                return loadedLabels;
            }
            
            var requiredLabelStrings = requiredLabels.Select(l => l.labelString).ToHashSet();
            return loadedLabels.Where(label => !requiredLabelStrings.Contains(label.labelString)).ToList();
        }
        
        /// <summary>
        /// Compares current and new label lists and returns only the labels that need to be loaded
        /// </summary>
        /// <param name="arm">ARM instance</param>
        /// <param name="currentLabels">Currently loaded/used labels</param>
        /// <param name="newLabels">New labels that will be needed</param>
        /// <returns>List of labels that need to be loaded</returns>
        public static List<AssetLabelReference> CompareAndGetLabelsToLoad(this ARM arm, 
            List<AssetLabelReference> currentLabels, 
            List<AssetLabelReference> newLabels)
        {
            if (currentLabels == null || currentLabels.Count == 0)
            {
                return arm.GetLabelsToLoad(newLabels);
            }
            
            if (newLabels == null || newLabels.Count == 0)
            {
                return new List<AssetLabelReference>();
            }

            var currentLabelStrings = currentLabels.Select(l => l.labelString).ToHashSet();
            var uniqueNewLabels = newLabels.Where(label => !currentLabelStrings.Contains(label.labelString)).ToList();
            return arm.GetLabelsToLoad(uniqueNewLabels);
        }
        
        /// <summary>
        /// Compares current and new label lists and returns labels that need to be unloaded
        /// </summary>
        /// <param name="arm">ARM instance</param>
        /// <param name="currentLabels">Currently loaded/used labels</param>
        /// <param name="newLabels">New labels that will be needed</param>
        /// <returns>List of labels that should be unloaded</returns>
        public static List<AssetLabelReference> CompareAndGetLabelsToUnload(this ARM arm, 
            List<AssetLabelReference> currentLabels, 
            List<AssetLabelReference> newLabels)
        {
            if (currentLabels == null || currentLabels.Count == 0)
            {
                return new List<AssetLabelReference>();
            }

            var newLabelStrings = newLabels?.Select(l => l.labelString).ToHashSet() ?? new HashSet<string>();
            return currentLabels.Where(label => !newLabelStrings.Contains(label.labelString)).ToList();
        }
    }
}
#endif