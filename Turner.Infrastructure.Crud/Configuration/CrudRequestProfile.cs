using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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

        internal abstract ICrudRequestConfig BuildConfiguration();

        internal abstract void Apply<TRequestConfig>(CrudRequestConfig<TRequestConfig> config);
    }
    
    public abstract class CrudRequestProfileCommon<TRequest> 
        : CrudRequestProfile
    {
        internal readonly Dictionary<Type, ICrudRequestEntityConfigBuilder> _requestEntityBuilders
            = new Dictionary<Type, ICrudRequestEntityConfigBuilder>();

        private List<CrudRequestProfile> _inheritProfiles 
            = new List<CrudRequestProfile>();

        private Action<CrudRequestOptionsConfig> _optionsConfig;
        private Action<CrudRequestErrorConfig> _errorConfig;
 
        protected internal readonly List<IRequestHookFactory> RequestHooks
            = new List<IRequestHookFactory>();

        protected internal readonly List<IResultHookFactory> ResultHooks
            = new List<IResultHookFactory>();

        public override Type RequestType => typeof(TRequest);

        internal override void Inherit(IEnumerable<CrudRequestProfile> profiles)
        {
            _inheritProfiles = profiles.ToList();
        }

        internal override ICrudRequestConfig BuildConfiguration()
        {
            var config = (CrudRequestConfig<TRequest>)Activator.CreateInstance(
                typeof(CrudRequestConfig<>).MakeGenericType(typeof(TRequest)));
            
            foreach (var profile in _inheritProfiles)
                profile.Apply(config);

            return config;
        }

        internal override void Apply<TConfigRequest>(CrudRequestConfig<TConfigRequest> config)
        {
            if (_optionsConfig != null)
            {
                var options = new CrudRequestOptionsConfig();
                _optionsConfig(options);
                config.SetOptions(options);
            }
            
            config.AddRequestHooks(RequestHooks);

            config.AddResultHooks(ResultHooks);
            
            ApplyErrorConfig(config);

            foreach (var builder in _requestEntityBuilders.Values)
                builder.Build(config);
        }

        protected void AddRequestHook<THook, TBaseRequest>()
            where THook : IRequestHook<TBaseRequest>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddRequestHook), typeof(TBaseRequest), typeof(TRequest));

            RequestHooks.Add(TypeRequestHookFactory.From<THook, TBaseRequest>());
        }

        protected void AddRequestHook<THook>()
            where THook : IRequestHook<TRequest>
            => AddRequestHook<THook, TRequest>();

        protected void AddRequestHook<TBaseRequest>(IRequestHook<TBaseRequest> hook)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddRequestHook), typeof(TBaseRequest), typeof(TRequest));

            RequestHooks.Add(InstanceRequestHookFactory.From(hook));
        }

        protected void AddRequestHook(Func<TRequest, CancellationToken, Task> hook)
        {
            RequestHooks.Add(FunctionRequestHookFactory.From(hook));
        }

        protected void AddRequestHook(Func<TRequest, Task> hook)
            => AddRequestHook((request, ct) => hook(request));

        protected void AddRequestHook(Action<TRequest> hook)
        {
            RequestHooks.Add(FunctionRequestHookFactory.From(hook));
        }

        protected void AddResultHook<THook, TBaseRequest, TResult>()
            where THook : IResultHook<TBaseRequest, TResult>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddResultHook), typeof(TBaseRequest), typeof(TRequest));

            ResultHooks.Add(TypeResultHookFactory.From<THook, TBaseRequest, TResult>());
        }

        protected void AddResultHook<THook, TResult>()
            where THook : IResultHook<TRequest, TResult>
            => AddResultHook<THook, TRequest, TResult>();

        protected void AddResultHook<TBaseRequest, TResult>(IResultHook<TBaseRequest, TResult> hook)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddResultHook), typeof(TBaseRequest), typeof(TRequest));

            ResultHooks.Add(InstanceResultHookFactory.From(hook));
        }

        protected void AddResultHook<TResult>(Func<TRequest, TResult, CancellationToken, Task<TResult>> hook)
        {
            ResultHooks.Add(FunctionResultHookFactory.From(hook));
        }

        protected void AddResultHook<TResult>(Func<TRequest, TResult, Task<TResult>> hook)
            => AddResultHook<TResult>((request, result, ct) => hook(request, result));

        protected void AddResultHook<TResult>(Func<TRequest, TResult, TResult> hook)
        {
            ResultHooks.Add(FunctionResultHookFactory.From(hook));
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

        public CrudBulkRequestProfile(Expression<Func<TRequest, IEnumerable<TItem>>> defaultItemSource)
        {
            _defaultItemSource = defaultItemSource;
        }

        protected CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>();

            if (_defaultItemSource != null)
                builder.WithRequestItems(_defaultItemSource);

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
