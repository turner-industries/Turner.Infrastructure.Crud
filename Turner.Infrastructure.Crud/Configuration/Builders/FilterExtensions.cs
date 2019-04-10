using System;
using System.Linq;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class FunctionFilterExtensions
    {
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterUsing<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterUsing<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterOn<TRequest, TEntity, TRequestProp, TEntityProp>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TRequestProp>> requestFilterExpr,
            Expression<Func<TEntity, TEntityProp>> entityPropExpr)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(CreateFilterFunc(requestFilterExpr, entityPropExpr)));
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterOn<TRequest, TItem, TEntity, TRequestProp, TEntityProp>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, TRequestProp>> requestFilterExpr,
            Expression<Func<TEntity, TEntityProp>> entityPropExpr)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(CreateFilterFunc(requestFilterExpr, entityPropExpr)));
        }

        private static Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> CreateFilterFunc<TRequest, TEntity, TRequestProp, TEntityProp>(
            Expression<Func<TRequest, TRequestProp>> requestFilterExpr,
            Expression<Func<TEntity, TEntityProp>> entityPropExpr)
            where TEntity : class
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
            
            if (isOptional)
            {
                return Expression
                    .Lambda<Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>>>(
                        Expression.Condition(
                            isNullable
                                ? (Expression)Expression.Property(rFilterExpr, "HasValue")
                                : Expression.NotEqual(rPropExpr, Expression.Constant(null, typeof(TRequestProp))),
                            Expression.Call(whereInfo, qParamExpr, whereClause),
                            qParamExpr),
                        rParamExpr,
                        qParamExpr)
                    .Compile();
            }
            else
            {
                return Expression
                    .Lambda<Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>>>(
                        Expression.Call(whereInfo, qParamExpr, whereClause),
                        rParamExpr,
                        qParamExpr)
                    .Compile();
            }
        }
    }
}