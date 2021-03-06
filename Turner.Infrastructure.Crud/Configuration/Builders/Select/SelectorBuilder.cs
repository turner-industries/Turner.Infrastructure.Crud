﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Select
{
    public class SelectorBuilder<TRequest, TEntity>
        where TEntity : class
    {
        public ISelector Single(Func<TRequest, Expression<Func<TEntity, bool>>> selector)
        {
            return Selector.From(selector);
        }

        public ISelector Single(Expression<Func<TRequest, TEntity, bool>> selector)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var body = selector.Body.ReplaceParameter(selector.Parameters[0], rParamExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(body, selector.Parameters[1]);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Single<TRequestKey, TEntityKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var rKeyExpr = Expression.Invoke(requestKeyExpr, rParamExpr);
            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Single<TRequestKey, TEntityKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr,
            Expression<Func<TRequestKey, TEntityKey, bool>> compareExpr)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var rKeyExpr = Expression.Invoke(requestKeyExpr, rParamExpr);
            var doCompareExpr = Expression.Invoke(compareExpr, eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(doCompareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Single<TRequestKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            string entityKeyProperty)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var rKeyExpr = Expression.Invoke(requestKeyExpr, rParamExpr);
            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }
        
        public ISelector Single<TEntityKey>(string requestKeyProperty,
            Expression<Func<TRequest, TEntityKey>> entityKeyExpr)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var rKeyExpr = Expression.PropertyOrField(rParamExpr, requestKeyProperty);
            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }
        
        public ISelector Single(string requestKeyProperty, string entityKeyProperty)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var rKeyExpr = Expression.PropertyOrField(rParamExpr, requestKeyProperty);
            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Single<TKey>(string requestKeyProperty, string entityKeyProperty,
            Expression<Func<TKey, TKey, bool>> compareExpr)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var rKeyExpr = Expression.PropertyOrField(rParamExpr, requestKeyProperty);
            var doCompareExpr = Expression.Invoke(compareExpr, eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(doCompareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Single(string keyProperty)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, keyProperty);
            var rKeyExpr = Expression.PropertyOrField(rParamExpr, keyProperty);
            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Single<TKey>(string keyProperty, Expression<Func<TKey, TKey, bool>> compareExpr)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, keyProperty);
            var rKeyExpr = Expression.PropertyOrField(rParamExpr, keyProperty);
            var doCompareExpr = Expression.Invoke(compareExpr, eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(doCompareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Collection<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Collection<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Collection<TIn, TKey>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var whereInfo = enumerableMethods
                .Single(x => x.Name == "Where" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn));

            var rWhereParam = Expression.Parameter(typeof(TIn));
            var compareExpr = Expression.NotEqual(rWhereParam, Expression.Constant(default(TIn), typeof(TIn)));
            var whereLambda = Expression.Lambda(compareExpr, rWhereParam);

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), typeof(TKey));

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rWhereExpr = Expression.Call(whereInfo, reExpr, whereLambda);
            var rReduceExpr = Expression.Call(selectInfo, rWhereExpr, requestItemKeyExpr);
            var rContainsExpr = Expression.Call(containsInfo, rReduceExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Collection<TIn>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var whereInfo = enumerableMethods
                .Single(x => x.Name == "Where" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn));

            var rWhereParam = Expression.Parameter(typeof(TIn));
            var compareExpr = Expression.NotEqual(rWhereParam, Expression.Constant(default(TIn), typeof(TIn)));
            var whereLambda = Expression.Lambda(compareExpr, rWhereParam);

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

            var rWhereExpr = Expression.Call(whereInfo, reExpr, whereLambda);
            var rReduceExpr = Expression.Call(selectInfo, rWhereExpr, iExpr);
            var rContainsExpr = Expression.Call(containsInfo, rReduceExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        internal ISelector Single(IKey requestKey, IKey entityKey)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var eParamExpr = Expression.Parameter(typeof(TEntity));

            var rKeyExpr = Expression.Invoke(requestKey.KeyExpression, rParamExpr);
            var eKeyExpr = Expression.Invoke(entityKey.KeyExpression, eParamExpr);
            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        internal ISelector Collection<TIn>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            IKey entityKey,
            IKey itemKey)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var eKeyExpr = Expression.Invoke(entityKey.KeyExpression, eParamExpr);
            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var whereInfo = enumerableMethods
                .Single(x => x.Name == "Where" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn));

            var rWhereParam = Expression.Parameter(typeof(TIn));
            var compareExpr = Expression.NotEqual(rWhereParam, Expression.Constant(default(TIn), typeof(TIn)));
            var whereLambda = Expression.Lambda(compareExpr, rWhereParam);

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), itemKey.KeyType);

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(itemKey.KeyType);

            var rWhereExpr = Expression.Call(whereInfo, reExpr, whereLambda);
            var rReduceExpr = Expression.Call(selectInfo, rWhereExpr, itemKey.KeyExpression);
            var rContainsExpr = Expression.Call(containsInfo, rReduceExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }
    }
}
