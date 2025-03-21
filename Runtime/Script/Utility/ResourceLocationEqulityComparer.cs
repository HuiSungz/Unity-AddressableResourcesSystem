
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AddressableManage
{
    /// <summary>
    /// IResourceLocation에 대한 내용 기반 동등성 비교기
    /// </summary>
    internal class ResourceLocationEqualityComparer : IEqualityComparer<IResourceLocation>
    {
        public static readonly ResourceLocationEqualityComparer Instance = new();
        
        public bool Equals(IResourceLocation compareX, IResourceLocation compareY)
        {
            if (ReferenceEquals(compareX, compareY)) return true;
            if (compareX == null || compareY == null) return false;

            if (compareX.PrimaryKey != compareY.PrimaryKey) return false;
            if (compareX.ResourceType != compareY.ResourceType) return false;
            return compareX.InternalId == compareY.InternalId;
        }
        
        public int GetHashCode(IResourceLocation obj)
        {
            var hashCode = 17;
            hashCode = hashCode * 23 + (obj.PrimaryKey?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (obj.ResourceType?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (obj.InternalId?.GetHashCode() ?? 0);
            
            return hashCode;
        }
    }
}