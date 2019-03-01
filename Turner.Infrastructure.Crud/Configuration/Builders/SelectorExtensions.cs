using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class SelectorExtensions
    {
        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder>(
            this CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class
            where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Single(selector));
        }

        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder>(
            this CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TEntity, bool>> selector)
            where TEntity : class
            where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Single(selector));
        }

        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder, TKey>(
            this CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Collection(requestEnumerableExpr, entityKeyExpr));
        }

        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder, TIn, TKey>(
            this CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Collection(requestEnumerableExpr, requestItemKeyExpr, entityKeyExpr));
        }
    }
}
