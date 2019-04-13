using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Context
{
    internal static class QueryableExtensions
    {
        private static readonly MethodInfo _firstOrDefault 
            = GetMethod(nameof(Queryable.FirstOrDefault));

        private static readonly MethodInfo _firstOrDefaultPredicate 
            = GetMethod(nameof(Queryable.FirstOrDefault), 1);
        
        private static readonly MethodInfo _singleOrDefault 
            = GetMethod(nameof(Queryable.SingleOrDefault));

        private static readonly MethodInfo _singleOrDefaultPredicate 
            = GetMethod(nameof(Queryable.SingleOrDefault), 1);
        
        private static readonly MethodInfo _count 
            = GetMethod(nameof(Queryable.Count));

        private static readonly MethodInfo _countPredicate
            = GetMethod(nameof(Queryable.Count), 1);
        
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>(_firstOrDefault, source, token);

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>(_firstOrDefaultPredicate, source, predicate, token);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, 
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>(_singleOrDefault, source, token);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>(_singleOrDefaultPredicate, source, predicate, token);

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, int>(_count, source, token);

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, int>(_countPredicate, source, predicate, token);

        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => source.AsAsyncEnumerable().ToList(token);

        public static Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => source.AsAsyncEnumerable().ToArray(token);

        private static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> source)
        {
            if (source is IAsyncEnumerable<TSource> enumerable)
                return enumerable;

            if (source is IAsyncEnumerableAccessor<TSource> accessor)
                return accessor.AsyncEnumerable;

            throw new ArgumentException($"'{nameof(source)}' is not async.");
        }

        private static Task<TResult> ExecuteAsync<TSource, TResult>(MethodInfo method,
            IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            token.ThrowIfCancellationRequested();

            if (!(source.Provider is IEntityQueryProvider queryProvider))
                throw new InvalidQueryProviderTypeException();

            if (method.IsGenericMethod)
                method = method.MakeGenericMethod(typeof(TSource));
                
            return queryProvider.ExecuteAsync<TResult>(
                Expression.Call(null, method, source.Expression),
                token);
        }

        private static Task<TResult> ExecuteAsync<TSource, TResult>(MethodInfo method,
            IQueryable<TSource> source,
            LambdaExpression expression,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TResult>(method, source, Expression.Quote(expression), token);

        private static Task<TResult> ExecuteAsync<TSource, TResult>(
            MethodInfo method,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken token = default(CancellationToken))
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            token.ThrowIfCancellationRequested();

            if (!(source.Provider is IEntityQueryProvider queryProvider))
                throw new InvalidQueryProviderTypeException();

            method = method.GetGenericArguments().Length == 2
                ? method.MakeGenericMethod(typeof(TSource), typeof(TResult))
                : method.MakeGenericMethod(typeof(TSource));

            return queryProvider.ExecuteAsync<TResult>(
                Expression.Call(null, method, new[] { source.Expression, expression }),
                token);
        }

        private static MethodInfo GetMethod<TResult>(string name,
            int parameterCount = 0,
            Func<MethodInfo, bool> predicate = null)
        {
            return GetMethod(name, parameterCount, 
                x => x.ReturnType == typeof(TResult) && (predicate == null || predicate(x)));
        }

        private static MethodInfo GetMethod(string name,
            int parameterCount = 0,
            Func<MethodInfo, bool> predicate = null)
        {
            return typeof(Queryable)
                .GetTypeInfo()
                .GetDeclaredMethods(name)
                .Single(x => x.GetParameters().Length == parameterCount + 1
                          && (predicate == null || predicate(x)));
        }
    }
}
