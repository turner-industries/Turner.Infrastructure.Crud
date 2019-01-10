using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<Tuple<TLeft, TRight>> Cartesian<TLeft, TRight>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right)
        {
            return left.Join(right, x => true, y => true, (x, y) => new Tuple<TLeft, TRight>(x, y));
        }

        internal static IEnumerable<Tuple<TLeft, TRight>> FullOuterJoin<TLeft, TRight, TKey>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector)
        {
            var leftLookup = left.ToLookup(leftKeySelector);
            var rightLookup = right.ToLookup(rightKeySelector);

            var keys = new HashSet<TKey>(leftLookup.Select(p => p.Key));
            keys.UnionWith(rightLookup.Select(p => p.Key));

            return keys.SelectMany(key =>
                leftLookup[key].DefaultIfEmpty(default(TLeft))
                .Cartesian(rightLookup[key].DefaultIfEmpty(default(TRight))));
        }
    }
}
