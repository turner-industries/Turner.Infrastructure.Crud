﻿using System;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Select
{
    public class SelectorBuilder<TRequest, TEntity>
        where TEntity : class
    {
        public ISelector Build(Func<TRequest, Expression<Func<TEntity, bool>>> selector)
        {
            return Selector.From(selector);
        }

        public ISelector Build<TRequestKey, TEntityKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.Invoke(requestKeyExpr, rParamExpr);
                var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            });
        }

        public ISelector Build<TRequestKey, TEntityKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr,
            Expression<Func<TRequestKey, TEntityKey, bool>> compareExpr)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.Invoke(requestKeyExpr, rParamExpr);
                var doCompareExpr = Expression.Invoke(compareExpr, eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(doCompareExpr, eParamExpr);
            });
        }

        public ISelector Build<TRequestKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            string entityKeyProperty)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.Invoke(requestKeyExpr, rParamExpr);
                var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            });
        }
        
        public ISelector Build<TEntityKey>(string requestKeyProperty,
            Expression<Func<TRequest, TEntityKey>> entityKeyExpr)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.PropertyOrField(rParamExpr, requestKeyProperty);
                var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            });
        }
        
        public ISelector Build(string requestKeyProperty, string entityKeyProperty)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.PropertyOrField(rParamExpr, requestKeyProperty);
                var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            });
        }

        public ISelector Build<TKey>(string requestKeyProperty, string entityKeyProperty,
            Expression<Func<TKey, TKey, bool>> compareExpr)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.PropertyOrField(rParamExpr, requestKeyProperty);
                var doCompareExpr = Expression.Invoke(compareExpr, eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(doCompareExpr, eParamExpr);
            });
        }

        public ISelector Build(string keyProperty)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.PropertyOrField(eParamExpr, keyProperty);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.PropertyOrField(rParamExpr, keyProperty);
                var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            });
        }

        public ISelector Build<TKey>(string keyProperty, Expression<Func<TKey, TKey, bool>> compareExpr)
        {
            return Selector.From<TRequest, TEntity>(request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.PropertyOrField(eParamExpr, keyProperty);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.PropertyOrField(rParamExpr, keyProperty);
                var doCompareExpr = Expression.Invoke(compareExpr, eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(doCompareExpr, eParamExpr);
            });
        }
    }
}