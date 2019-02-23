using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Filter
{
    public abstract class FilterBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        internal abstract IFilterFactory Build();
    }

    public class FilterBuilder<TRequest, TEntity>
        where TEntity : class
    {
        private FilterBuilderBase<TRequest, TEntity> _builder;

        public BasicFilterBuilder<TRequest, TEntity> Using(
            Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> filterFunc)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> Using(
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) => queryable.Where(filterFunc(request)));

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> Using(
            Expression<Func<TRequest, TEntity, bool>> filterExpr)
        {
            return Using(request =>
            {
                var requestParam = Expression.Constant(request, typeof(TRequest));
                var body = filterExpr.Body.ReplaceParameter(filterExpr.Parameters[0], requestParam);

                return Expression.Lambda<Func<TEntity, bool>>(body, filterExpr.Parameters[1]);
            });
        }

        public BasicFilterBuilder<TRequest, TEntity> Using(
            Expression<Func<TEntity, bool>> filterExpr)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) => queryable.Where(filterExpr));

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> On<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "request");
            var qParamExpr = Expression.Parameter(typeof(IQueryable<TEntity>), "queryable");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "entity");

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);
            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));
            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);

            var builder = CreateFilterBuilder(qParamExpr, rParamExpr, whereClause);
            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> On<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var qParamExpr = Expression.Parameter(typeof(IQueryable<TEntity>));
            var eParamExpr = Expression.Parameter(typeof(TEntity));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);

            var builder = CreateFilterBuilder(qParamExpr, rParamExpr, whereClause);
            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> On<TIn, TKey>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var qParamExpr = Expression.Parameter(typeof(IQueryable<TEntity>));
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

            var builder = CreateFilterBuilder(qParamExpr, rParamExpr, whereClause);
            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> On<TIn>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var qParamExpr = Expression.Parameter(typeof(IQueryable<TEntity>));
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

            var builder = CreateFilterBuilder(qParamExpr, rParamExpr, whereClause);
            _builder = builder;

            return builder;
        }

        public DateFilterBuilder<TRequest, TEntity> Include(Expression<Func<TEntity, DateTime>> entityDateExpr)
        {
            var builder = new DateFilterBuilder<TRequest, TEntity>(entityDateExpr, true);
            _builder = builder;

            return builder;
        }

        public DateFilterBuilder<TRequest, TEntity> Exclude(Expression<Func<TEntity, DateTime>> entityDateExpr)
        {
            var builder = new DateFilterBuilder<TRequest, TEntity>(entityDateExpr, false);
            _builder = builder;

            return builder;
        }

        public StringFilterBuilder<TRequest, TEntity> Include(Expression<Func<TEntity, string>> entityStringExpr)
        {
            var builder = new StringFilterBuilder<TRequest, TEntity>(entityStringExpr, true);
            _builder = builder;

            return builder;
        }

        public StringFilterBuilder<TRequest, TEntity> Exclude(Expression<Func<TEntity, string>> entityStringExpr)
        {
            var builder = new StringFilterBuilder<TRequest, TEntity>(entityStringExpr, false);
            _builder = builder;

            return builder;
        }
        
        internal IFilterFactory Build()
        {
            return _builder?.Build();
        }

        private BasicFilterBuilder<TRequest, TEntity> CreateFilterBuilder(
            ParameterExpression queryableExpr,
            ParameterExpression requestExpr,
            Expression<Func<TEntity, bool>> whereClause)
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

            return new BasicFilterBuilder<TRequest, TEntity>(filterLambda.Compile());
        }
    }
}
