using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
