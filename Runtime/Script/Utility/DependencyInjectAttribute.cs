
using System;

namespace AddressableManage
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor)]
    internal class DependencyInjectAttribute : Attribute { }
}