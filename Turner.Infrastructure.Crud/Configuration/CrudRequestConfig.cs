using System;
using System.Collections.Generic;
using AutoMapper;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestConfig
    {
        TEntity CreateEntity<TEntity>(object request) 
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private readonly Dictionary<Type, Func<TRequest, object>> _entityCreators
            = new Dictionary<Type, Func<TRequest, object>>();

        internal void SetEntityCreator<TEntity>(Func<TRequest, TEntity> creator)
        {
            _entityCreators[typeof(TEntity)] = request => creator(request);
        }

        public TEntity CreateEntity<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest tRequest))
                throw new BadCrudConfigurationException();

            // TODO: Should search for TEntity base types as well
            if (!_entityCreators.TryGetValue(typeof(TEntity), out var creator))
                throw new BadCrudConfigurationException();

            return (TEntity) creator(tRequest);
        }
    }

    public class DefaultCrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        public TEntity CreateEntity<TEntity>(object request) where TEntity : class
        {
            return DefaultCreateEntity<TEntity>(request);
        }

        public static TEntity DefaultCreateEntity<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest tRequest))
                throw new BadCrudConfigurationException();

            return Mapper.Map<TEntity>(tRequest);
        }
    }
}
