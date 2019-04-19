using System.Collections.Generic;

namespace Turner.Infrastructure.Crud.Extensions
{
    public static class TupleExtensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue val)
        {
            key = kvp.Key;
            val = kvp.Value;
        }
    }
}
