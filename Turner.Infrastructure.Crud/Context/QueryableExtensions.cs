using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Context
{
    internal static class QueryableExtensions
    {
        private static readonly Dictionary<string, MethodInfo> _methods =
            new Dictionary<string, MethodInfo>
            {
                { "FirstOrDefault", GetMethod(nameof(Queryable.FirstOrDefault)) },
                { "FirstOrDefaultPredicate", GetMethod(nameof(Queryable.FirstOrDefault), 1) },
                { "SingleOrDefault", GetMethod(nameof(Queryable.SingleOrDefault))},
                { "SingleOrDefaultPredicate", GetMethod(nameof(Queryable.SingleOrDefault), 1) },
                { "Count", GetMethod(nameof(Queryable.Count)) },
                { "CountPredicate", GetMethod(nameof(Queryable.Count), 1) }
            };
        
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>("FirstOrDefault", source, token);

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>("FirstOrDefaultPredicate", source, predicate, token);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, 
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>("SingleOrDefault", source, token);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TSource>("SingleOrDefaultPredicate", source, predicate, token);

        public static Task<TResult> ProjectSingleOrDefaultAsync<TSource, TResult>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>().SingleOrDefaultAsync(token);

        public static Task<TResult> ProjectSingleOrDefaultAsync<TSource, TResult>(this IQueryable<TSource> source,
            Expression<Func<TResult, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>().SingleOrDefaultAsync(predicate, token);

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, int>("Count", source, token);

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, int>("CountPredicate", source, predicate, token);

        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => source.AsAsyncEnumerable().ToList(token);

        public static Task<List<TResult>> ProjectToListAsync<TSource, TResult>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>().ToListAsync(token);

        public static Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => source.AsAsyncEnumerable().ToArray(token);

        public static Task<TResult[]> ProjectToArrayAsync<TSource, TResult>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>().ToArrayAsync(token);

        private static Task<TResult> ExecuteAsync<TSource, TResult>(string methodName,
            IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            token.ThrowIfCancellationRequested();

            if (!(source.Provider is IAsyncQueryProvider queryProvider))
                throw new InvalidQueryProviderTypeException();

            var method = _methods[methodName] ?? throw new ArgumentOutOfRangeException(methodName);
            if (method.IsGenericMethod)
                method = method.MakeGenericMethod(typeof(TSource));
                
            return queryProvider.ExecuteAsync<TResult>(
                Expression.Call(null, method, source.Expression),
                token);
        }

        private static Task<TResult> ExecuteAsync<TSource, TResult>(string methodName,
            IQueryable<TSource> source,
            LambdaExpression expression,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TResult>(methodName, source, Expression.Quote(expression), token);

        private static Task<TResult> ExecuteAsync<TSource, TResult>(
            string methodName,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken token = default(CancellationToken))
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            token.ThrowIfCancellationRequested();

            if (!(source.Provider is IAsyncQueryProvider queryProvider))
                throw new InvalidQueryProviderTypeException();

            var method = _methods[methodName] ?? throw new ArgumentOutOfRangeException(methodName);
            method = method.GetGenericArguments().Length == 2
                ? method.MakeGenericMethod(typeof(TSource), typeof(TResult))
                : method.MakeGenericMethod(typeof(TSource));

            return queryProvider.ExecuteAsync<TResult>(
                Expression.Call(null, method, new[] { source.Expression, expression }),
                token);
        }

        private static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> source)
        {
            if (source is IAsyncEnumerable<TSource> enumerable)
                return enumerable;

            if (source is IAsyncEnumerableAccessor<TSource> accessor)
                return accessor.AsyncEnumerable;

            throw new ArgumentException($"'{nameof(source)}' is not async.");
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
