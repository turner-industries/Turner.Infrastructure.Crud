using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class FilterUsingExtensions
    {
        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
            => FilterUsing(config, filterFunc, null);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
            => FilterUsing(config, filterFunc, null);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing(config, filterExpr, null);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing(config, filterExpr, null);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing<TRequest, TEntity, RequestEntityConfigBuilder<TRequest, TEntity>>(config, filterExpr, null);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing<TRequest, TEntity, BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>>(config, filterExpr, null);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
            => FilterUsing(config, filterFunc, predicateFunc);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
            => FilterUsing(config, filterFunc, predicateFunc);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TRequest, TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing(config, filterExpr, predicateFunc);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TRequest, TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing(config, filterExpr, predicateFunc);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing(config, filterExpr, predicateFunc);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TEntity, bool>> filterExpr)
            where TEntity : class
            => FilterUsing(config, filterExpr, predicateFunc);

        private static TBuilder FilterUsing<TRequest, TEntity, TBuilder>(
            TBuilder config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return predicateFunc == null
                ? config.FilterWith((request, queryable) => queryable.Where(filterFunc(request)))
                : config.FilterWith((request, queryable) =>
                {
                    return predicateFunc(request) ? queryable.Where(filterFunc(request)) : queryable;
                    });
        }

        private static TBuilder FilterUsing<TRequest, TEntity, TBuilder>(
            TBuilder config,
            Expression<Func<TRequest, TEntity, bool>> filterExpr,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return FilterUsing(config, request =>
            {
                var requestParam = Expression.Constant(request, typeof(TRequest));
                var body = filterExpr.Body.ReplaceParameter(filterExpr.Parameters[0], requestParam);

                return Expression.Lambda<Func<TEntity, bool>>(body, filterExpr.Parameters[1]);
            }, predicateFunc);
        }

        private static TBuilder FilterUsing<TRequest, TEntity, TBuilder>(
            TBuilder config,
            Expression<Func<TEntity, bool>> filterExpr,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return predicateFunc == null
                ? config.FilterWith((request, queryable) => queryable.Where(filterExpr))
                : config.FilterWith((request, queryable) => predicateFunc(request) ? queryable.Where(filterExpr) : queryable);
        }
    }

    public static class FilterOnExtensions
    {
        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterOn<TRequest, TEntity, TRequestProp, TEntityProp>(
           this RequestEntityConfigBuilder<TRequest, TEntity> config,
           Expression<Func<TRequest, TRequestProp>> requestFilterExpr,
           Expression<Func<TEntity, TEntityProp>> entityPropExpr)
           where TEntity : class
            => FilterOn<TRequest, TEntity, RequestEntityConfigBuilder<TRequest, TEntity>, TRequestProp, TEntityProp>(config, requestFilterExpr, entityPropExpr);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterOn<TRequest, TItem, TEntity, TRequestProp, TEntityProp>(
           this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
           Expression<Func<TRequest, TRequestProp>> requestFilterExpr,
           Expression<Func<TEntity, TEntityProp>> entityPropExpr)
           where TEntity : class
            => FilterOn<TRequest, TEntity, BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>, TRequestProp, TEntityProp>(config, requestFilterExpr, entityPropExpr);
        
        public static TBuilder FilterOn<TRequest, TEntity, TBuilder, TKey>(
            this TBuilder config,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
            => FilterOn(config, null, requestEnumerableExpr, entityKeyExpr);

        public static TBuilder FilterOn<TRequest, TEntity, TBuilder, TKey>(
            this TBuilder config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
            => FilterOn(config, requestEnumerableExpr, entityKeyExpr, predicateFunc);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterOn<TRequest, TEntity, TIn>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
            where TEntity : class
            => FilterOn<TRequest, TEntity, RequestEntityConfigBuilder<TRequest, TEntity>, TIn>(config, requestEnumerableExpr, requestItemKeyProperty, entityKeyProperty, null);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterOn<TRequest, TEntity, TIn>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            => FilterOn<TRequest, TEntity, RequestEntityConfigBuilder<TRequest, TEntity>, TIn>(config, requestEnumerableExpr, requestItemKeyProperty, entityKeyProperty, predicateFunc);

        public static TBuilder FilterOn<TRequest, TEntity, TBuilder, TIn, TKey>(
            this TBuilder config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
            => FilterOn(config, requestEnumerableExpr, requestItemKeyExpr, entityKeyExpr, null);

        public static TBuilder FilterOn<TRequest, TEntity, TBuilder, TIn, TKey>(
            this TBuilder config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
            => FilterOn(config, requestEnumerableExpr, requestItemKeyExpr, entityKeyExpr, predicateFunc);
        
        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterOn<TRequest, TItem, TEntity, TIn>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
            where TEntity : class
            => FilterOn<TRequest, TEntity, BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>, TIn>(config, requestEnumerableExpr, requestItemKeyProperty, entityKeyProperty, null);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterOn<TRequest, TItem, TEntity, TIn>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
            where TEntity : class
            => FilterOn<TRequest, TEntity, BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>, TIn>(config, requestEnumerableExpr, requestItemKeyProperty, entityKeyProperty, predicateFunc);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterOn<TRequest, TEntity, TKey>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
            where TEntity : class
            => FilterOn<TRequest, TEntity, RequestEntityConfigBuilder<TRequest, TEntity>, TKey>(config, requestEnumerableExpr, entityKeyProperty, null);

        public static RequestEntityConfigBuilder<TRequest, TEntity> FilterOn<TRequest, TEntity, TKey>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
            where TEntity : class
            => FilterOn<TRequest, TEntity, RequestEntityConfigBuilder<TRequest, TEntity>, TKey>(config, requestEnumerableExpr, entityKeyProperty, predicateFunc);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterOn<TRequest, TItem, TEntity, TKey>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
            where TEntity : class
            => FilterOn<TRequest, TEntity, BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>, TKey>(config, requestEnumerableExpr, entityKeyProperty, null);

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterOn<TRequest, TItem, TEntity, TKey>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, bool> predicateFunc,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
            where TEntity : class
            => FilterOn<TRequest, TEntity, BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>, TKey>(config, requestEnumerableExpr, entityKeyProperty, predicateFunc);

        private static TBuilder FilterOn<TRequest, TEntity, TBuilder, TRequestProp, TEntityProp>(
            TBuilder config,
            Expression<Func<TRequest, TRequestProp>> requestFilterExpr,
            Expression<Func<TEntity, TEntityProp>> entityPropExpr)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            bool isNullable = Nullable.GetUnderlyingType(typeof(TRequestProp)) != null;
            bool isOptional = isNullable || !typeof(TRequestProp).IsValueType;

            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var qParamExpr = Expression.Parameter(typeof(IQueryable<TEntity>));
            var eParamExpr = Expression.Parameter(typeof(TEntity));

            var ePropExpr = Expression.Invoke(entityPropExpr, eParamExpr);
            var rFilterExpr = Expression.Invoke(requestFilterExpr, rParamExpr);
            var rPropExpr = isNullable
                ? Expression.Property(rFilterExpr, "Value")
                : (Expression)rFilterExpr;

            var compareExpr = Expression.Equal(rPropExpr, ePropExpr);
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);

            var whereInfo = typeof(Queryable)
                .GetMethods()
                .Single(x => x.Name == "Where"
                    && x.GetParameters().Length == 2
                    && x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1
                    && x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TEntity));

            var filterFunc = isOptional
                ? Expression
                    .Lambda<Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>>>(
                        Expression.Condition(
                            isNullable
                                ? (Expression)Expression.Property(rFilterExpr, "HasValue")
                                : Expression.NotEqual(rPropExpr, Expression.Constant(null, typeof(TRequestProp))),
                            Expression.Call(whereInfo, qParamExpr, whereClause),
                            qParamExpr),
                        rParamExpr,
                        qParamExpr)
                    .Compile()
                : Expression
                    .Lambda<Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>>>(
                        Expression.Call(whereInfo, qParamExpr, whereClause),
                        rParamExpr,
                        qParamExpr)
                    .Compile();

            return config.FilterWith(filterFunc);
        }

        private static TBuilder FilterOn<TRequest, TEntity, TBuilder, TKey>(
            TBuilder config,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var eParamExpr = Expression.Parameter(typeof(TEntity));

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);
            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));
            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);

            var filterFunc = CreateFilterFunc<TRequest, TEntity>(
                Expression.Parameter(typeof(IQueryable<TEntity>)),
                rParamExpr,
                Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr));

            return predicateFunc == null
                ? config.FilterWith(filterFunc)
                : config.FilterWith((request, queryable) => predicateFunc(request) ? filterFunc(request, queryable) : queryable);
        }

        private static TBuilder FilterOn<TRequest, TEntity, TBuilder, TIn, TKey>(
            TBuilder config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var eParamExpr = Expression.Parameter(typeof(TEntity));

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), typeof(TKey));

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var reReduceExpr = Expression.Call(selectInfo, reExpr, requestItemKeyExpr);
            var rContainsExpr = Expression.Call(containsInfo, reReduceExpr, eKeyExpr);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);

            var filterFunc = CreateFilterFunc<TRequest, TEntity>(
                Expression.Parameter(typeof(IQueryable<TEntity>)),
                rParamExpr,
                whereClause);

            return predicateFunc == null
                ? config.FilterWith(filterFunc)
                : config.FilterWith((request, queryable) => predicateFunc(request) ? filterFunc(request, queryable) : queryable);
        }

        private static TBuilder FilterOn<TRequest, TEntity, TBuilder, TIn>(
            TBuilder config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var eParamExpr = Expression.Parameter(typeof(TEntity));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), eKeyExpr.Type);

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(eKeyExpr.Type);

            var iParamExpr = Expression.Parameter(typeof(TIn));
            var iKeyExpr = Expression.PropertyOrField(iParamExpr, requestItemKeyProperty);
            var iExpr = Expression.Lambda(iKeyExpr, iParamExpr);

            var reReduceExpr = Expression.Call(selectInfo, reExpr, iExpr);
            var rContainsExpr = Expression.Call(containsInfo, reReduceExpr, eKeyExpr);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);

            var filterFunc = CreateFilterFunc<TRequest, TEntity>(
                Expression.Parameter(typeof(IQueryable<TEntity>)),
                rParamExpr,
                whereClause);
            
            return predicateFunc == null
                ? config.FilterWith(filterFunc)
                : config.FilterWith((request, queryable) => predicateFunc(request) ? filterFunc(request, queryable) : queryable);
        }

        private static TBuilder FilterOn<TRequest, TEntity, TBuilder, TKey>(
            TBuilder config,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty,
            Func<TRequest, bool> predicateFunc)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var eParamExpr = Expression.Parameter(typeof(TEntity));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);

            var filterFunc = CreateFilterFunc<TRequest, TEntity>(
                Expression.Parameter(typeof(IQueryable<TEntity>)),
                rParamExpr,
                whereClause);

            return predicateFunc == null
                ? config.FilterWith(filterFunc)
                : config.FilterWith((request, queryable) => predicateFunc(request) ? filterFunc(request, queryable) : queryable);
        }

        private static Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> CreateFilterFunc<TRequest, TEntity>(
            ParameterExpression queryableExpr,
            ParameterExpression requestExpr,
            Expression<Func<TEntity, bool>> whereClause)
            where TEntity : class
        {
            var whereInfo = typeof(Queryable)
                .GetMethods()
                .Single(x => x.Name == "Where"
                    && x.GetParameters().Length == 2
                    && x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1
                    && x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TEntity));

            var filterLambda = Expression.Lambda<Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>>>(
                Expression.Call(whereInfo, queryableExpr, whereClause),
                requestExpr,
                queryableExpr);

            return filterLambda.Compile();
        }
    }
}