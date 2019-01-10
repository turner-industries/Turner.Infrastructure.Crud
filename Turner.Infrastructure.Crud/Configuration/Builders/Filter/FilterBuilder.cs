using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Filter
{
    public abstract class FilterBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        internal abstract IFilter Build();
    }

    public class FilterBuilder<TRequest, TEntity>
        where TEntity : class
    {
        private FilterBuilderBase<TRequest, TEntity> _builder;

        public BasicFilterBuilder<TRequest, TEntity> FilterOn(
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) => queryable.Where(filterFunc(request)));

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn(
            Expression<Func<TRequest, TEntity, bool>> filterExpr)
        {
            return FilterOn(request =>
            {
                var requestParam = Expression.Constant(request, typeof(TRequest));
                var body = filterExpr.Body.ReplaceParameter(filterExpr.Parameters[0], requestParam);

                return Expression.Lambda<Func<TEntity, bool>>(body, filterExpr.Parameters[1]);
            });
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn(
            Expression<Func<TEntity, bool>> filterExpr)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) => queryable.Where(filterExpr));

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) =>
                {
                    var eParamExpr = Expression.Parameter(typeof(TEntity));
                    var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
                    var rParamExpr = Expression.Constant(request);
                    var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

                    var containsInfo = typeof(Enumerable)
                        .GetMethods()
                        .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(TKey));
                    
                    var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);
                    var whereClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);

                    return queryable.Where(whereClause);
                });

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) =>
                {
                    var eParamExpr = Expression.Parameter(typeof(TEntity));
                    var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
                    var rParamExpr = Expression.Constant(request);
                    var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

                    var containsInfo = typeof(Enumerable)
                        .GetMethods()
                        .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(TKey));

                    var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);
                    var whereClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);

                    return queryable.Where(whereClause);
                });

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn<TIn, TKey>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) =>
                {
                    var eParamExpr = Expression.Parameter(typeof(TEntity));
                    var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
                    var rParamExpr = Expression.Constant(request);
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

                    return queryable.Where(whereClause);
                });

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn<TIn>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) =>
                {
                    var eParamExpr = Expression.Parameter(typeof(TEntity));
                    var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
                    var rParamExpr = Expression.Constant(request);
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

                    return queryable.Where(whereClause);
                });

            _builder = builder;

            return builder;
        }

        public CustomFilterBuilder<TRequest, TEntity> FilterWith(
            Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> customFilterFunc)
        {
            var builder = new CustomFilterBuilder<TRequest, TEntity>(customFilterFunc);
            _builder = builder;

            return builder;
        }

        internal IFilter Build()
        {
            return _builder?.Build();
        }
    }
}
