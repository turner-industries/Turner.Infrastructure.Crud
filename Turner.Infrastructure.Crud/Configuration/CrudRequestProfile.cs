using System;
using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Crud.Configuration.Builders;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestProfile
    {
        Type RequestType { get; }

        void Inherit(IEnumerable<ICrudRequestProfile> profile);

        void Apply(ICrudRequestConfig config);
        void Apply<TRequest>(CrudRequestConfig<TRequest> config);
    }
    
    public abstract class CrudRequestProfile<TRequest> 
        : ICrudRequestProfile
    {
        protected readonly Dictionary<Type, ICrudRequestEntityConfigBuilder> RequestEntityBuilders
            = new Dictionary<Type, ICrudRequestEntityConfigBuilder>();

        protected readonly List<ICrudRequestProfile> _inheritProfiles 
            = new List<ICrudRequestProfile>();

        public Type RequestType => typeof(TRequest);

        public void Inherit(IEnumerable<ICrudRequestProfile> profiles)
        {
            _inheritProfiles.AddRange(profiles.Distinct());
        }

        protected CrudRequestEntityConfigBuilder<TRequest, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudRequestEntityConfigBuilder<TRequest, TEntity>();
            RequestEntityBuilders[typeof(TEntity)] = builder;

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
            var inheritedTypes = new List<Type>();
            foreach (var profile in _inheritProfiles)
            {
                if (inheritedTypes.Contains(profile.RequestType))
                    continue;

                profile.Apply(config);
                inheritedTypes.Add(profile.RequestType);
            }

            foreach (var builder in RequestEntityBuilders.Values)
                builder.Build(config);
        }
    }

    public class DefaultCrudRequestProfile<TRequest>
        : CrudRequestProfile<TRequest>
    {

    }
}
