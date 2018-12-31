using System;
using System.Collections.Generic;

namespace Turner.Infrastructure.Crud.Tests
{
    public static class Extensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
                yield return item;
            }
        }
    }
}
