using System;
using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestProfile
    {
        Type RequestType { get; }

        // TODO: Protect these functions
        void Inherit(IEnumerable<ICrudRequestProfile> profile);

        void Apply(ICrudRequestConfig config);
        void Apply<TRequest>(CrudRequestConfig<TRequest> config);
    }
    
    public abstract class CrudRequestProfile<TRequest> 
        : ICrudRequestProfile
    {
        private readonly Dictionary<Type, ICrudRequestEntityConfigBuilder> _requestEntityBuilders
            = new Dictionary<Type, ICrudRequestEntityConfigBuilder>();

        private readonly List<ICrudRequestProfile> _inheritProfiles 
            = new List<ICrudRequestProfile>();

        private Action<CrudRequestErrorConfig> _errorConfig;

        public Type RequestType => typeof(TRequest);

        public void Inherit(IEnumerable<ICrudRequestProfile> profiles)
        {
            _inheritProfiles.AddRange(profiles);
        }

        protected void ConfigureErrors(Action<CrudRequestErrorConfig> config)
        {
            _errorConfig = config;
        }

        protected CrudRequestEntityConfigBuilder<TRequest, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudRequestEntityConfigBuilder<TRequest, TEntity>();
            _requestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }
        
        public void Apply(ICrudRequestConfig config)
        {
            if (!(config is CrudRequestConfig<TRequest> tConfig))
            {
                var message = "Apply() should only be called internally.";
                throw new BadCrudConfigurationException(message);
            }

            Apply(tConfig);
        }

        public void Apply<TPerspective>(CrudRequestConfig<TPerspective> config)
        {
            foreach (var profile in _inheritProfiles.Distinct())
                profile.Apply(config);

            ApplyErrorConfig(config);

            foreach (var builder in _requestEntityBuilders.Values)
                builder.Build(config);
        }

        private void ApplyErrorConfig<TPerspective>(CrudRequestConfig<TPerspective> config)
        {
            var errorConfig = new CrudRequestErrorConfig();
            _errorConfig?.Invoke(errorConfig);

            if (errorConfig.FailedToFindIsError.HasValue)
                config.SetFailedToFindIsError(errorConfig.FailedToFindIsError.Value);
        }
    }

    public class DefaultCrudRequestProfile<TRequest>
        : CrudRequestProfile<TRequest>
    {

    }
}
