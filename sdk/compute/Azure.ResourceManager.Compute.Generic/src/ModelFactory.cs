// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Azure.ResourceManager.Compute
{
    /// <summary> Creates model instances from property bags. </summary>
    public static class ModelFactory
    {
        /// <summary> Creates a model with the supplied property values. </summary>
        /// <typeparam name="T"> The model type. </typeparam>
        /// <param name="properties"> An object whose public properties identify values to set. </param>
        /// <param name="current"> An optional model to shallow-copy before applying the values. </param>
        /// <returns> A new model instance. </returns>
        public static T Create<T>(object properties, T current = default)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            object model = current is null ? CreateUninitialized(typeof(T)) : ShallowClone(current);
            ApplyProperties(model, properties);
            return (T)model;
        }

        private static void ApplyProperties(object target, object properties)
        {
            foreach (PropertyInfo sourceProperty in properties.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                PropertyInfo targetProperty = target.GetType().GetProperty(sourceProperty.Name, BindingFlags.Instance | BindingFlags.Public)
                    ?? throw new ArgumentException($"Property '{sourceProperty.Name}' does not exist on {target.GetType().FullName}.", nameof(properties));

                object value = ConvertValue(sourceProperty.GetValue(properties), targetProperty.PropertyType, sourceProperty.Name);
                if (!TrySetProperty(target, targetProperty, value))
                {
                    throw new ArgumentException($"Property '{sourceProperty.Name}' cannot be initialized on {target.GetType().FullName}.", nameof(properties));
                }
            }
        }

        private static object ConvertValue(object value, Type targetType, string propertyName)
        {
            if (value is null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null)
                {
                    throw new ArgumentException($"Null is not valid for property '{propertyName}'.", "properties");
                }
                return null;
            }

            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }

            if (IsPropertyBag(value.GetType()) && !targetType.IsValueType && targetType != typeof(string))
            {
                object nested = CreateUninitialized(targetType);
                ApplyProperties(nested, value);
                return nested;
            }

            throw new ArgumentException($"Value for property '{propertyName}' must be assignable to {targetType.FullName}.", "properties");
        }

        private static bool TrySetProperty(object target, PropertyInfo property, object value)
        {
            MethodInfo setter = property.GetSetMethod(nonPublic: true);
            if (setter is not null)
            {
                setter.Invoke(target, new[] { value });
                return true;
            }

            FieldInfo field = FindField(target.GetType(), $"<{property.Name}>k__BackingField");
            if (field is not null)
            {
                field.SetValue(target, value);
                return true;
            }

            PropertyInfo containerProperty = target.GetType().GetProperty("Properties", BindingFlags.Instance | BindingFlags.NonPublic);
            if (containerProperty is null)
            {
                return false;
            }

            object container = containerProperty.GetValue(target);
            container = container is null ? CreateUninitialized(containerProperty.PropertyType) : ShallowClone(container);
            if (!TrySetProperty(target, containerProperty, container))
            {
                return false;
            }

            PropertyInfo nestedProperty = container.GetType().GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return nestedProperty is not null && TrySetProperty(container, nestedProperty, value);
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (Type current = type; current is not null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field is not null)
                {
                    return field;
                }
            }
            return null;
        }

        private static bool IsPropertyBag(Type type) =>
            type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false)
            && type.Name.Contains("AnonymousType");

        private static object ShallowClone(object value) =>
            typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(value, null);

        private static object CreateUninitialized(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters)
            {
                throw new ArgumentException($"Type {type.FullName} cannot be created.", "properties");
            }

#pragma warning disable SYSLIB0050 // Prototype must construct models without accessible constructors.
            object instance = FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050
            InitializeCollections(instance);
            return instance;
        }

        private static void InitializeCollections(object instance)
        {
            for (Type current = instance.GetType(); current is not null; current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (field.GetValue(instance) is null && TryCreateCollection(field.FieldType, out object collection))
                    {
                        field.SetValue(instance, collection);
                    }
                }
            }
        }

        private static bool TryCreateCollection(Type type, out object collection)
        {
            collection = null;
            if (type.IsArray)
            {
                collection = Array.CreateInstance(type.GetElementType(), 0);
                return true;
            }

            if (!type.IsGenericType)
            {
                return false;
            }

            Type definition = type.GetGenericTypeDefinition();
            Type[] arguments = type.GetGenericArguments();
            Type implementation = null;
            if (arguments.Length == 1 && (definition == typeof(IList<>)
                || definition == typeof(IReadOnlyList<>) || definition == typeof(IEnumerable<>)))
            {
                implementation = typeof(List<>).MakeGenericType(arguments);
            }
            else if (arguments.Length == 2 && (definition == typeof(IDictionary<,>)
                || definition == typeof(IReadOnlyDictionary<,>)))
            {
                implementation = typeof(Dictionary<,>).MakeGenericType(arguments);
            }

            if (implementation is null || !type.IsAssignableFrom(implementation))
            {
                return false;
            }

            collection = Activator.CreateInstance(implementation);
            return true;
        }
    }
}
