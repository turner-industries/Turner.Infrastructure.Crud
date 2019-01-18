using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class CrudRequestProfile
    {
        public abstract Type RequestType { get; }
        
        internal abstract void Inherit(IEnumerable<CrudRequestProfile> profile);

        internal abstract void Apply(ICrudRequestConfig config);

        internal abstract void Apply<TRequest>(CrudRequestConfig<TRequest> config, ref List<Type> inherited);
    }
    
    public abstract class CrudRequestProfileCommon<TRequest> 
        : CrudRequestProfile
    {
        internal readonly Dictionary<Type, ICrudRequestEntityConfigBuilder> _requestEntityBuilders
            = new Dictionary<Type, ICrudRequestEntityConfigBuilder>();

        private readonly List<CrudRequestProfile> _inheritProfiles 
            = new List<CrudRequestProfile>();

        private Action<CrudRequestOptionsConfig> _optionsConfig;
        private Action<CrudRequestErrorConfig> _errorConfig;
 
        protected readonly List<IRequestHookFactory> RequestHooks
            = new List<IRequestHookFactory>();
        
        public override Type RequestType => typeof(TRequest);

        internal override void Inherit(IEnumerable<CrudRequestProfile> profiles)
        {
            _inheritProfiles.AddRange(profiles);
        }

        internal override void Apply(ICrudRequestConfig config)
        {
            if (!(config is CrudRequestConfig<TRequest> tConfig))
            {
                const string message = "Apply() should only be called internally.";
                throw new BadCrudConfigurationException(message);
            }

            var inherited = new List<Type>();

            Apply(tConfig, ref inherited);
        }

        internal override void Apply<TPerspective>(CrudRequestConfig<TPerspective> config, ref List<Type> inherited)
        {
            foreach (var profile in _inheritProfiles)
            {
                if (inherited.Contains(profile.RequestType))
                    continue;

                inherited.Add(profile.RequestType);

                profile.Apply(config, ref inherited);
            }

            if (_optionsConfig != null)
            {
                var options = new CrudRequestOptionsConfig();
                _optionsConfig(options);
                config.SetOptions(options);
            }
            
            config.AddRequestHooks(RequestHooks);
            
            ApplyErrorConfig(config);

            foreach (var builder in _requestEntityBuilders.Values)
                builder.Build(config);
        }

        protected void WithRequestHook<THook>()
            where THook : IRequestHook<TRequest>
        {
            RequestHooks.Add(TypeRequestHookFactory.From<THook, TRequest>());
        }

        protected void WithRequestHook(IRequestHook<TRequest> hook)
        {
            RequestHooks.Add(InstanceRequestHookFactory.From(hook));
        }

        protected void WithRequestHook(Func<TRequest, Task> hook)
        {
            RequestHooks.Add(FunctionRequestHookFactory.From(hook));
        }

        protected void WithRequestHook(Action<TRequest> hook)
        {
            RequestHooks.Add(FunctionRequestHookFactory.From(hook));
        }

        protected void ConfigureOptions(Action<CrudRequestOptionsConfig> config)
        {
            _optionsConfig = config;
        }

        protected void ConfigureErrors(Action<CrudRequestErrorConfig> config)
        {
            _errorConfig = config;
        }
        
        private void ApplyErrorConfig<TPerspective>(CrudRequestConfig<TPerspective> config)
        {
            var errorConfig = new CrudRequestErrorConfig();
            _errorConfig?.Invoke(errorConfig);

            if (errorConfig.FailedToFindInGetIsError.HasValue)
                config.ErrorConfig.FailedToFindInGetIsError = errorConfig.FailedToFindInGetIsError.Value;

            if (errorConfig.FailedToFindInGetAllIsError.HasValue)
                config.ErrorConfig.FailedToFindInGetAllIsError = errorConfig.FailedToFindInGetAllIsError.Value;

            if (errorConfig.FailedToFindInFindIsError.HasValue)
                config.ErrorConfig.FailedToFindInFindIsError = errorConfig.FailedToFindInFindIsError.Value;

            if (errorConfig.FailedToFindInUpdateIsError.HasValue)
                config.ErrorConfig.FailedToFindInUpdateIsError = errorConfig.FailedToFindInUpdateIsError.Value;

            if (errorConfig.FailedToFindInDeleteIsError.HasValue)
                config.ErrorConfig.FailedToFindInDeleteIsError = errorConfig.FailedToFindInDeleteIsError.Value;

            if (errorConfig.ErrorHandlerFactory != null)
                config.ErrorConfig.SetErrorHandler(errorConfig.ErrorHandlerFactory);
        }
    }
    
    public abstract class CrudRequestProfile<TRequest>
        : CrudRequestProfileCommon<TRequest>
    {
        public CrudRequestProfile()
        {
            if (typeof(IBulkRequest).IsAssignableFrom(typeof(TRequest)))
            {
                var message =
                    $"Unable to build configuration for request '{typeof(TRequest)}'." +
                    $"This request type should define a 'CrudBulkRequestProfile'.";

                throw new BadCrudConfigurationException(message);
            }
        }

        protected CrudRequestEntityConfigBuilder<TRequest, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudRequestEntityConfigBuilder<TRequest, TEntity>();
            _requestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }
    }

    public abstract class CrudBulkRequestProfile<TRequest, TItem>
        : CrudRequestProfileCommon<TRequest>
    {
        private readonly Expression<Func<TRequest, IEnumerable<TItem>>> _defaultItemSource;
        
        public CrudBulkRequestProfile()
        {
        }

        public CrudBulkRequestProfile(Expression<Func<TRequest, IEnumerable<TItem>>> defaultDataSource)
        {
            _defaultItemSource = defaultDataSource;
        }

        protected CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>();

            if (_defaultItemSource != null)
                builder.WithItems(_defaultItemSource);

            _requestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }
    }

    public class DefaultCrudRequestProfile<TRequest> : CrudRequestProfile<TRequest>
    {
    }

    public class DefaultBulkCrudRequestProfile<TRequest> : CrudBulkRequestProfile<TRequest, TRequest>
    {
    }
}
