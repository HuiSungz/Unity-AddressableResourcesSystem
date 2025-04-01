
using System;

namespace ArchitectHS.AddressableManage
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor)]
    internal class DependencyInjectAttribute : Attribute { }
}