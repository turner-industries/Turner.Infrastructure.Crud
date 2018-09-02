using System;
using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class CrudRequestProfile
    {
        public abstract Type RequestType { get; }
        
        internal abstract void Inherit(IEnumerable<CrudRequestProfile> profile);
        internal abstract void Apply(ICrudRequestConfig config);
        internal abstract void Apply<TRequest>(CrudRequestConfig<TRequest> config);
    }
    
    public abstract class CrudRequestProfile<TRequest> 
        : CrudRequestProfile
    {
        private readonly Dictionary<Type, ICrudRequestEntityConfigBuilder> _requestEntityBuilders
            = new Dictionary<Type, ICrudRequestEntityConfigBuilder>();

        private readonly List<CrudRequestProfile> _inheritProfiles 
            = new List<CrudRequestProfile>();

        private Action<CrudRequestErrorConfig> _errorConfig;

        public override Type RequestType => typeof(TRequest);

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
        
        internal override void Inherit(IEnumerable<CrudRequestProfile> profiles)
        {
            _inheritProfiles.AddRange(profiles);
        }

        internal override void Apply(ICrudRequestConfig config)
        {
            if (!(config is CrudRequestConfig<TRequest> tConfig))
            {
                var message = "Apply() should only be called internally.";
                throw new BadCrudConfigurationException(message);
            }

            Apply(tConfig);
        }

        internal override void Apply<TPerspective>(CrudRequestConfig<TPerspective> config)
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

            if (errorConfig.FailedToFindInGetIsError.HasValue)
                config.SetFailedToFindInGetIsError(errorConfig.FailedToFindInGetIsError.Value);

            if (errorConfig.FailedToFindInUpdateIsError.HasValue)
                config.SetFailedToFindInUpdateIsError(errorConfig.FailedToFindInUpdateIsError.Value);
        }
    }

    public class DefaultCrudRequestProfile<TRequest>
        : CrudRequestProfile<TRequest>
    {

    }
}
