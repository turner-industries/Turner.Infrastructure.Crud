using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    internal static class TypeExtensions
    {
        internal static IEnumerable<Type> BuildTypeHierarchyDown(this Type type)
        {
            var visited = new HashSet<Type> { type };
            var typeParents = new[] { type.BaseType }
                .Concat(type.GetInterfaces())
                .Where(x => x != null);

            foreach (var parent in typeParents)
            {
                foreach (var item in BuildTypeHierarchyDown(parent))
                {
                    if (visited.Contains(item))
                        continue;

                    visited.Add(item);
                    yield return item;
                }
            }

            yield return type;
        }

        internal static IEnumerable<Type> BuildTypeHierarchyUp(this Type type)
        {
            yield return type;

            var visited = new HashSet<Type> { type };
            var typeParents = new[] { type.BaseType }
                .Concat(type.GetInterfaces())
                .Where(x => x != null);

            foreach (var parent in typeParents)
            {
                foreach (var item in BuildTypeHierarchyUp(parent))
                {
                    if (visited.Contains(item))
                        continue;

                    visited.Add(item);
                    yield return item;
                }
            }
        }

        internal static Type GetSequenceType(this Type type)
            => TryGetSequenceType(type) 
                ?? throw new ArgumentException($"'{nameof(type)}' is not a sequence type.");
        
        internal static Type TryGetSequenceType(this Type type)
            => type.TryGetElementType(typeof(IEnumerable<>))
               ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

        internal static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (type.GetTypeInfo().IsGenericTypeDefinition)
                return null;

            var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

            Type singleImplementation = null;

            foreach (var implementation in types)
            {
                if (singleImplementation == null)
                    singleImplementation = implementation;
                else
                {
                    singleImplementation = null;
                    break;
                }
            }

            return singleImplementation?.GetTypeInfo().GenericTypeArguments.FirstOrDefault();
        }

        internal static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();

            if (!typeInfo.IsGenericTypeDefinition)
            {
                var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : type.GetBaseTypes();

                foreach (var baseType in baseTypes)
                {
                    if (baseType.GetTypeInfo().IsGenericType &&
                        baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return baseType;
                    }
                }

                if (type.GetTypeInfo().IsGenericType &&
                    type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
        }

        internal static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            type = type.GetTypeInfo().BaseType;

            while (type != null)
            {
                yield return type;

                type = type.GetTypeInfo().BaseType;
            }
        }
    }
}
