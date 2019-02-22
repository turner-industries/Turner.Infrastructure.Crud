using System;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class FunctionFilterExtensions
    {
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterWith<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterWith<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterWith<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterWith<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterWith<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> FilterWith<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TEntity, bool>> filterFunc)
            where TEntity : class
        {
            return config.FilterWith(builder => builder.Using(filterFunc));
        }
    }

    public static class DateFilterExtensions
    {
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> SearchBefore<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TEntity, DateTime>> entityDateExpr,
            DateTime value,
            bool ignoreTime = false)
            where TEntity : class
        {
            var valueParam = Expression.Constant(ignoreTime ? value.Date : value, typeof(DateTime));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var entityProp = ignoreTime
                ? Expression.Property(Expression.Invoke(entityDateExpr, entityParam), "Date")
                : (Expression)Expression.Invoke(entityDateExpr, entityParam);

            var compareExpr = Expression.LessThan(entityProp, valueParam);

            var filterExpr = Expression.Lambda<Func<TEntity, bool>>(compareExpr, entityParam);

            return config.FilterWith(filterExpr);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> SearchBeforeOrOn<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TEntity, DateTime>> entityDateExpr,
            DateTime value,
            bool ignoreTime = false)
            where TEntity : class
        {
            var valueParam = Expression.Constant(ignoreTime ? value.Date : value, typeof(DateTime));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var entityProp = ignoreTime
                ? Expression.Property(Expression.Invoke(entityDateExpr, entityParam), "Date")
                : (Expression)Expression.Invoke(entityDateExpr, entityParam);

            var compareExpr = Expression.LessThanOrEqual(entityProp, valueParam);

            var filterExpr = Expression.Lambda<Func<TEntity, bool>>(compareExpr, entityParam);

            return config.FilterWith(filterExpr);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> SearchBefore<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, DateTime>> requestDateExpr,
            Expression<Func<TEntity, DateTime>> entityDateExpr,
            bool ignoreTime = false)
            where TEntity : class
        {
            var requestParam = Expression.Parameter(typeof(DateTime));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var entityProp = ignoreTime
                ? Expression.Property(Expression.Invoke(entityDateExpr, entityParam), "Date")
                : (Expression)Expression.Invoke(entityDateExpr, entityParam);

            var compareExpr = Expression.LessThan(entityProp, requestParam);

            var filterExpr = Expression.Lambda<Func<TRequest, TEntity, bool>>(compareExpr, requestParam, entityParam);

            return config.FilterWith(filterExpr);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> SearchBeforeOrOn<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, DateTime>> requestDateExpr,
            Expression<Func<TEntity, DateTime>> entityDateExpr,
            bool ignoreTime = false)
            where TEntity : class
        {
            var requestParam = Expression.Parameter(typeof(DateTime));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var entityProp = ignoreTime
                ? Expression.Property(Expression.Invoke(entityDateExpr, entityParam), "Date")
                : (Expression)Expression.Invoke(entityDateExpr, entityParam);

            var compareExpr = Expression.LessThanOrEqual(entityProp, requestParam);

            var filterExpr = Expression.Lambda<Func<TRequest, TEntity, bool>>(compareExpr, requestParam, entityParam);

            return config.FilterWith(filterExpr);
        }

        // TODO: abstract commonality

        // TODO: nullable datetime

        // TODO: >

        // TODO: <,>

        // TODO: ==

        // TODO: ignoreTime option

        // TODO: Bulk config
    }
}