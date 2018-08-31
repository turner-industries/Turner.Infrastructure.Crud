using System;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration.Builders;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestProfile
    {
        ICrudRequestConfig BuildConfig();
    }
    
    public abstract class CrudRequestProfile<TRequest> 
        : ICrudRequestProfile
    {
        protected readonly Dictionary<Type, ICrudRequestEntityConfigBuilder> RequestEntityBuilders
            = new Dictionary<Type, ICrudRequestEntityConfigBuilder>();

        protected CrudRequestEntityConfigBuilder<TRequest, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudRequestEntityConfigBuilder<TRequest, TEntity>();
            RequestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }

        public ICrudRequestConfig BuildConfig()
        {
            var config = new CrudRequestConfig<TRequest>();

            foreach (var builder in RequestEntityBuilders.Values)
                builder.Build(config);

            return config;
        }
    }

    public class DefaultCrudRequestProfile<TRequest>
        : CrudRequestProfile<TRequest>
    {
    }
}
