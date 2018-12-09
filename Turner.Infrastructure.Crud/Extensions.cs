using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    internal static class TaskExtensions
    {
        internal static ConfiguredTaskAwaitable Configure(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        internal static ConfiguredTaskAwaitable<TResult> Configure<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }
    }

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

    internal static class ExpressionExtensions
    {
        internal static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
        {
            var replacer = new ParameterReplacer(source, target);

            return replacer.Visit(expression);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            public ParameterExpression Source { get; }

            public Expression Target { get; }

            public ParameterReplacer(ParameterExpression source, Expression target)
            {
                Source = source;
                Target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == Source ? Target : base.VisitParameter(node);
        }
    }
}
