using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class CrudRequestProfile
    {
        public abstract Type RequestType { get; }
        
        internal abstract void Inherit(IEnumerable<CrudRequestProfile> profile);
        internal abstract void Apply(ICrudRequestConfig config);
        internal abstract void Apply<TRequest>(CrudRequestConfig<TRequest> config, ref List<Type> inherited);
    }
    
    public abstract class CrudRequestProfile<TRequest> 
        : CrudRequestProfile
    {
        private readonly Dictionary<Type, ICrudRequestEntityConfigBuilder> _requestEntityBuilders
            = new Dictionary<Type, ICrudRequestEntityConfigBuilder>();

        private readonly List<CrudRequestProfile> _inheritProfiles 
            = new List<CrudRequestProfile>();

        private readonly List<Func<object, Task>> _preCreateActions
            = new List<Func<object, Task>>();

        private readonly List<Func<object, Task>> _preUpdateActions
            = new List<Func<object, Task>>();

        private readonly List<Func<object, Task>> _preDeleteActions
            = new List<Func<object, Task>>();

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

        protected void BeforeCreating(Func<TRequest, Task> preCreateAction)
        {
            if (preCreateAction != null)
                _preCreateActions.Add(request => preCreateAction((TRequest) request));
        }

        protected void BeforeCreating(Action<TRequest> preCreateAction)
        {
            if (preCreateAction != null)
                _preCreateActions.Add(request =>
                {
                    preCreateAction((TRequest) request);
                    return Task.CompletedTask;
                });
        }
        
        protected void BeforeUpdating(Func<TRequest, Task> preUpdateAction)
        {
            if (preUpdateAction != null)
                _preUpdateActions.Add(request => preUpdateAction((TRequest) request));
        }

        protected void BeforeUpdating(Action<TRequest> preUpdateAction)
        {
            if (preUpdateAction != null)
                _preUpdateActions.Add(request =>
                {
                    preUpdateAction((TRequest) request);
                    return Task.CompletedTask;
                });
        }

        protected void BeforeDeleting(Func<TRequest, Task> preDeleteAction)
        {
            if (preDeleteAction != null)
                _preDeleteActions.Add(request => preDeleteAction((TRequest)request));
        }

        protected void BeforeDeleting(Action<TRequest> preDeleteAction)
        {
            if (preDeleteAction != null)
                _preDeleteActions.Add(request =>
                {
                    preDeleteAction((TRequest)request);
                    return Task.CompletedTask;
                });
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

            config.AddPreCreateActions(_preCreateActions);
            config.AddPreUpdateActions(_preUpdateActions);
            config.AddPreDeleteActions(_preDeleteActions);

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

            if (errorConfig.FailedToFindInDeleteIsError.HasValue)
                config.SetFailedToFindInDeleteIsError(errorConfig.FailedToFindInDeleteIsError.Value);
        }
    }

    public class DefaultCrudRequestProfile<TRequest>
        : CrudRequestProfile<TRequest>
    {

    }
}
