
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ArchitectHS.AddressableManage
{
    internal class DependencyContainer
    {
        #region Fields & Constructor
        
        private readonly Dictionary<Type, object> _instances = new();
        private static DependencyContainer _instance;

        public static DependencyContainer Instance
        {
            get
            {
                _instance ??= new DependencyContainer();
                return _instance;
            }
        }
        
        #endregion
        
        public void Register<TInterface, TImplementation>(TImplementation implementation) where TImplementation : TInterface
        {
            _instances[typeof(TInterface)] = implementation;
        }
        
        public void Register<T>(T implementation)
        {
            _instances[typeof(T)] = implementation;
        }

        public T Resolve<T>()
        {
            if (_instances.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance;
            }

            throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the container");
        }
        
        public void InjectDependencies(object target)
        {
            var type = target.GetType();
            
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (!Attribute.IsDefined(field, typeof(DependencyInjectAttribute)))
                {
                    continue;
                }
                
                var fieldType = field.FieldType;
                if (_instances.TryGetValue(fieldType, out var dependency))
                {
                    field.SetValue(target, dependency);
                }
                else
                {
                    throw new InvalidOperationException($"Dependency of type {fieldType.Name} is not registered");
                }
            }
            
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (!Attribute.IsDefined(property, typeof(DependencyInjectAttribute)) || !property.CanWrite)
                {
                    continue;
                }
                
                var propertyType = property.PropertyType;
                if (_instances.TryGetValue(propertyType, out var dependency))
                {
                    property.SetValue(target, dependency);
                }
                else
                {
                    throw new InvalidOperationException($"Dependency of type {propertyType.Name} is not registered");
                }
            }
        }
    }
}