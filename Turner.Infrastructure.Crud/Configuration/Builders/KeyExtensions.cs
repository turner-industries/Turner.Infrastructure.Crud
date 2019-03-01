using System;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class KeyExtensions
    {
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> UseKeys<TRequest, TEntity, TItemKey, TEntityKey>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TItemKey>> requestKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
            where TEntity : class
        {
            return config
                .UseEntityKey(entityKeyExpr)
                .UseRequestKey(requestKeyExpr);
        }
        
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> UseKeys<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            string requestKeyProperty,
            string entityKeyProperty)
            where TEntity : class
        {
            return config
                .UseEntityKey(entityKeyProperty)
                .UseRequestKey(requestKeyProperty);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> UseKeys<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            string keyProperty)
            where TEntity : class
        => config.UseKeys(keyProperty, keyProperty);

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseKeys<TRequest, TItem, TEntity, TItemKey, TEntityKey>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TItem, TItemKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
            where TEntity : class
        {
            return config
                .UseEntityKey(entityKeyExpr)
                .UseRequestItemKey(requestItemKeyExpr);
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseKeys<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            string itemKeyProperty,
            string entityKeyProperty)
            where TEntity : class
        {
            return config
                .UseEntityKey(entityKeyProperty)
                .UseRequestItemKey(itemKeyProperty);
        }
        
        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseKeys<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            string keyProperty)
            where TEntity : class
        => config.UseKeys(keyProperty, keyProperty);
    }
}
