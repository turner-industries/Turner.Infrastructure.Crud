using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class RequestProfile
    {
        public abstract Type RequestType { get; }
        
        internal abstract void Inherit(IEnumerable<RequestProfile> profile);

        internal abstract IRequestConfig BuildConfiguration();

        internal abstract void Apply<TRequestConfig>(RequestConfig<TRequestConfig> config);
    }
    
    public abstract class RequestProfileCommon<TRequest> 
        : RequestProfile
        where TRequest : ICrudRequest
    {
        internal readonly Dictionary<Type, IRequestEntityConfigBuilder> RequestEntityBuilders
            = new Dictionary<Type, IRequestEntityConfigBuilder>();

        private List<RequestProfile> _inheritProfiles 
            = new List<RequestProfile>();

        private Action<RequestOptionsConfig> _optionsConfig;
        private Action<RequestErrorConfig> _errorConfig;
 
        protected internal readonly List<IRequestHookFactory> RequestHooks
            = new List<IRequestHookFactory>();

        protected internal readonly List<IResultHookFactory> ResultHooks
            = new List<IResultHookFactory>();

        public override Type RequestType => typeof(TRequest);

        internal override void Inherit(IEnumerable<RequestProfile> profiles)
        {
            _inheritProfiles = profiles.ToList();
        }

        internal override IRequestConfig BuildConfiguration()
        {
            var config = (RequestConfig<TRequest>)Activator.CreateInstance(
                typeof(RequestConfig<>).MakeGenericType(typeof(TRequest)));
            
            foreach (var profile in _inheritProfiles)
                profile.Apply(config);

            return config;
        }

        internal override void Apply<TConfigRequest>(RequestConfig<TConfigRequest> config)
        {
            if (_optionsConfig != null)
            {
                var options = new RequestOptionsConfig();
                _optionsConfig(options);
                config.SetOptions(options);
            }
            
            config.AddRequestHooks(RequestHooks);

            config.AddResultHooks(ResultHooks);
            
            ApplyErrorConfig(config);

            foreach (var builder in RequestEntityBuilders.Values)
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

        protected void ConfigureOptions(Action<RequestOptionsConfig> config)
        {
            _optionsConfig = config;
        }

        protected void ConfigureErrors(Action<RequestErrorConfig> config)
        {
            _errorConfig = config;
        }
        
        private void ApplyErrorConfig<TPerspective>(RequestConfig<TPerspective> config)
        {
            var errorConfig = new RequestErrorConfig();
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
    
    public abstract class RequestProfile<TRequest>
        : RequestProfileCommon<TRequest>
        where TRequest : ICrudRequest
    {
        public RequestProfile()
        {
            if (typeof(IBulkRequest).IsAssignableFrom(typeof(TRequest)) &&
                !typeof(TRequest).IsInterface &&
                !typeof(TRequest).IsAbstract &&
                !typeof(TRequest).IsGenericTypeDefinition)
            {
                var message =
                    $"Unable to build configuration for request '{typeof(TRequest)}'." +
                    $"This request type should define a 'CrudBulkRequestProfile'.";

                throw new BadConfigurationException(message);
            }
        }

        protected RequestEntityConfigBuilder<TRequest, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new RequestEntityConfigBuilder<TRequest, TEntity>();
            RequestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }
    }
    
    public class DefaultCrudRequestProfile<TRequest> : RequestProfile<TRequest>
        where TRequest : ICrudRequest
    {
    }
}
