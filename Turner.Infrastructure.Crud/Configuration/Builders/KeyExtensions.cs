using System;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class KeyExtensions
    {
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> WithKeys<TRequest, TEntity, TItemKey, TEntityKey>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TItemKey>> requestKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
            where TEntity : class
        {
            return config
                .WithEntityKey(entityKeyExpr)
                .WithRequestKey(requestKeyExpr);
        }
        
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> WithKeys<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            string requestKeyProperty,
            string entityKeyProperty)
            where TEntity : class
        {
            return config
                .WithEntityKey(entityKeyProperty)
                .WithRequestKey(requestKeyProperty);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> WithKeys<TRequest, TEntity>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            string keyProperty)
            where TEntity : class
        => config.WithKeys(keyProperty, keyProperty);

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithKeys<TRequest, TItem, TEntity, TItemKey, TEntityKey>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TItem, TItemKey>> itemKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
            where TEntity : class
        {
            return config
                .WithEntityKey(entityKeyExpr)
                .WithRequestKey(itemKeyExpr);
        }

        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithKeys<TRequest, TItem, TEntity>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            string itemKeyProperty,
            string entityKeyProperty)
            where TEntity : class
        {
            return config
                .WithEntityKey(entityKeyProperty)
                .WithRequestKey(itemKeyProperty);
        }
        
        public static CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithKeys<TEntity, TItem, TRequest>(
            this CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            string keyProperty)
            where TEntity : class
        => config.WithKeys(keyProperty, keyProperty);
    }
}
