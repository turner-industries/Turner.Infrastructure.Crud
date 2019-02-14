using System;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class FilterExtensions
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
    }
}