﻿using System;
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

        private readonly Dictionary<ActionType, ActionList> _preActions
            = new Dictionary<ActionType, ActionList>();

        private readonly Dictionary<ActionType, ActionList> _postActions
            = new Dictionary<ActionType, ActionList>();

        private Action<CrudOptionsConfig> _optionsConfig;
        private Action<CrudRequestErrorConfig> _errorConfig;
        
        protected CrudRequestProfileCommon()
        {
            foreach (var type in (ActionType[])Enum.GetValues(typeof(ActionType)))
            {
                _preActions[type] = new ActionList();
                _postActions[type] = new ActionList();
            }
        }
        
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
                var options = new CrudOptionsConfig();
                _optionsConfig(options);
                config.SetOptions(options);
            }

            foreach (var type in (ActionType[])Enum.GetValues(typeof(ActionType)))
            {
                config.AddPreActions(type, _preActions[type]);
                config.AddPostActions(type, _postActions[type]);
            }

            ApplyErrorConfig(config);

            foreach (var builder in _requestEntityBuilders.Values)
                builder.Build(config);
        }

        protected void ConfigureOptions(Action<CrudOptionsConfig> config)
        {
            _optionsConfig = config;
        }

        protected void ConfigureErrors(Action<CrudRequestErrorConfig> config)
        {
            _errorConfig = config;
        }
        
        protected void BeforeCreating(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Create, action);

        protected void BeforeCreating(Action<TRequest> action)
            => AddPreAction(ActionType.Create, action);

        protected void AfterCreating(Func<TRequest, Task> action)
            => AddPostAction(ActionType.Create, action);

        protected void AfterCreating(Action<TRequest> action)
            => AddPostAction(ActionType.Create, action);

        protected void BeforeUpdating(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Update, action);

        protected void BeforeUpdating(Action<TRequest> action)
            => AddPreAction(ActionType.Update, action);

        protected void AfterUpdating(Func<TRequest, Task> action)
            => AddPostAction(ActionType.Update, action);

        protected void AfterUpdating(Action<TRequest> action)
            => AddPostAction(ActionType.Update, action);

        protected void BeforeDeleting(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Delete, action);

        protected void BeforeDeleting(Action<TRequest> action)
            => AddPreAction(ActionType.Delete, action);

        protected void AfterDeleting(Func<TRequest, Task> action)
            => AddPostAction(ActionType.Delete, action);

        protected void AfterDeleting(Action<TRequest> action)
            => AddPostAction(ActionType.Delete, action);
        
        protected void BeforeSaving(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Save, action);

        protected void BeforeSaving(Action<TRequest> action)
            => AddPreAction(ActionType.Save, action);

        protected void AfterSaving(Func<TRequest, Task> action)
            => AddPostAction(ActionType.Save, action);

        protected void AfterSaving(Action<TRequest> action)
            => AddPostAction(ActionType.Save, action);
        
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

        private void AddPreAction(ActionType type, Func<TRequest, Task> action)
        {
            if (action != null)
                _preActions[type].Add(request => action((TRequest) request));
        }

        private void AddPreAction(ActionType type, Action<TRequest> action)
        {
            if (action != null)
                _preActions[type].Add(request =>
                {
                    action((TRequest) request);
                    return Task.CompletedTask;
                });
        }

        private void AddPostAction(ActionType type, Func<TRequest, Task> action)
        {
            if (action != null)
                _postActions[type].Add(request => action((TRequest)request));
        }

        private void AddPostAction(ActionType type, Action<TRequest> action)
        {
            if (action != null)
                _postActions[type].Add(request =>
                {
                    action((TRequest)request);
                    return Task.CompletedTask;
                });
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
        protected CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>();
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
