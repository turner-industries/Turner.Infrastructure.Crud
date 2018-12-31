using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class UpdateAllExtensions
    {
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateAllWith<TRequest, TEntity, TIn, TKey>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
        {
            return UpdateAllWith(config, requestEnumerableExpr, requestItemKeyExpr, entityKeyExpr, Mapper.Map);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateAllWith<TRequest, TEntity, TIn, TKey>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr,
            Func<TIn, TEntity, TEntity> updator)
            where TEntity : class
        {
            config.SelectWith(builder => builder.Collection(requestEnumerableExpr, requestItemKeyExpr, entityKeyExpr));

            var rParamExpr = Expression.Parameter(typeof(TRequest));
            
            var enumerableMethods = typeof(Enumerable).GetMethods();

            var tupleType = typeof(Tuple<,>).MakeGenericType(typeof(TIn), typeof(TEntity));
            var tupleCtor = tupleType.GetConstructor(new[] { typeof(TIn), typeof(TEntity) });

            var selectTupleParam = Expression.Parameter(tupleType);
            var updateExpr = Expression.Call(
                updator.Method,
                Expression.PropertyOrField(selectTupleParam, "Item1"),
                Expression.PropertyOrField(selectTupleParam, "Item2"));
            var selectLambdaExpr = Expression.Lambda(updateExpr, selectTupleParam);

            var esParamExpr = Expression.Parameter(typeof(TEntity[]));

            var joinInfo = enumerableMethods
                .Single(x => x.Name == "Join" && x.GetParameters().Length == 5)
                .MakeGenericMethod(typeof(TEntity), typeof(TIn), typeof(TKey), tupleType);
            var joinOutEntityParam = Expression.Parameter(typeof(TEntity));
            var joinOutInParam = Expression.Parameter(typeof(TIn));
            var joinOutExpr = Expression.Lambda(
                Expression.New(tupleCtor, joinOutInParam, joinOutEntityParam),
                joinOutEntityParam,
                joinOutInParam);
            var joinInEnumParam = Expression.Invoke(requestEnumerableExpr, rParamExpr);
            var joinExpr = Expression.Call(joinInfo, esParamExpr, joinInEnumParam, entityKeyExpr, requestItemKeyExpr, joinOutExpr);

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(tupleType, typeof(TEntity));
            var selectExpr = Expression.Call(selectInfo, joinExpr, selectLambdaExpr);

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == "ToArray" && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TEntity));
            var toArrayExpr = Expression.Call(toArrayInfo, selectExpr);

            var lambdaExpr = Expression.Lambda(toArrayExpr, rParamExpr, esParamExpr);
            var lambda = (Func<TRequest, TEntity[], TEntity[]>)lambdaExpr.Compile();

            config.UpdateAllWith(lambda);

            return config;
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateAllWith<TRequest, TEntity, TIn>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
            where TEntity : class
        {
            return UpdateAllWith(config, requestEnumerableExpr, requestItemKeyProperty, entityKeyProperty, Mapper.Map);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateAllWith<TRequest, TEntity, TIn>(
            this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty,
            Func<TIn, TEntity, TEntity> updator)
            where TEntity : class
        {
            config.SelectWith(builder =>
                builder.Collection(requestEnumerableExpr, requestItemKeyProperty, entityKeyProperty));
            
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var tupleType = typeof(Tuple<,>).MakeGenericType(typeof(TIn), typeof(TEntity));
            var tupleCtor = tupleType.GetConstructor(new[] { typeof(TIn), typeof(TEntity) });

            var selectTupleParam = Expression.Parameter(tupleType);
            var updateExpr = Expression.Call(
                updator.Method,
                Expression.PropertyOrField(selectTupleParam, "Item1"),
                Expression.PropertyOrField(selectTupleParam, "Item2"));
            var selectLambdaExpr = Expression.Lambda(updateExpr, selectTupleParam);

            var esParamExpr = Expression.Parameter(typeof(TEntity[]));

            var joinInfo = enumerableMethods
                .Single(x => x.Name == "Join" && x.GetParameters().Length == 5)
                .MakeGenericMethod(typeof(TEntity), typeof(TIn), eKeyExpr.Type, tupleType);
            var joinEntityParam = Expression.Parameter(typeof(TEntity));
            var joinEntityKeyExpr = Expression.Lambda(
                Expression.PropertyOrField(joinEntityParam, entityKeyProperty),
                joinEntityParam);
            var joinInParam = Expression.Parameter(typeof(TIn));
            var joinInKeyExpr = Expression.Lambda(
                Expression.PropertyOrField(joinInParam, requestItemKeyProperty),
                joinInParam);
            var joinOutEntityParam = Expression.Parameter(typeof(TEntity));
            var joinOutInParam = Expression.Parameter(typeof(TIn));
            var joinOutExpr = Expression.Lambda(
                Expression.New(tupleCtor, joinOutInParam, joinOutEntityParam),
                joinOutEntityParam,
                joinOutInParam);
            var joinInEnumParam = Expression.Invoke(requestEnumerableExpr, rParamExpr);
            var joinExpr = Expression.Call(joinInfo, esParamExpr, joinInEnumParam, joinEntityKeyExpr, joinInKeyExpr, joinOutExpr);

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(tupleType, typeof(TEntity));
            var selectExpr = Expression.Call(selectInfo, joinExpr, selectLambdaExpr);

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == "ToArray" && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TEntity));
            var toArrayExpr = Expression.Call(toArrayInfo, selectExpr);

            var lambdaExpr = Expression.Lambda(toArrayExpr, rParamExpr, esParamExpr);
            var lambda = (Func<TRequest, TEntity[], TEntity[]>)lambdaExpr.Compile();

            config.UpdateAllWith(lambda);

            return config;
        }
    }
}
