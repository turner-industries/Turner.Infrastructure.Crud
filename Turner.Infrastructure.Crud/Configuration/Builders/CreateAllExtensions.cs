using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration
{
    public static class CreateAllExtensions
    {
        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateAllWith<TRequest, TEntity, TIn>(
                this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
                Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
                Func<TIn, TEntity> creator)
                where TEntity : class
        {
            var rParam = Expression.Parameter(typeof(TRequest));
            var itemsParam = Expression.Invoke(requestEnumerableExpr, rParam);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), typeof(TEntity));
            var selectParam = Expression.Parameter(typeof(TIn));
            var selectLambda = Expression.Lambda<Func<TIn, TEntity>>(
                Expression.Call(creator.Method, selectParam), selectParam);
            var selectExpr = Expression.Call(selectInfo, itemsParam, selectLambda);

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == "ToArray" && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TEntity));
            var toArrayExpr = Expression.Call(toArrayInfo, selectExpr);

            var lambdaExpr = Expression.Lambda(toArrayExpr, rParam);
            var lambda = (Func<TRequest, TEntity[]>) lambdaExpr.Compile();

            return config.CreateAllWith(lambda);
        }

        public static CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateAllWith<TRequest, TEntity, TIn>(
                this CrudRequestEntityConfigBuilder<TRequest, TEntity> config,
                Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr)
                where TEntity : class
        {
            return CreateAllWith(config, requestEnumerableExpr, Mapper.Map<TIn, TEntity>);
        }
    }
}
