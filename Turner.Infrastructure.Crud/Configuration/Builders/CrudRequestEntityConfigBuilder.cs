using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Utilities;

namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public interface ICrudRequestEntityConfigBuilder
    {
        void Build<TRequest>(CrudRequestConfig<TRequest> config);
    }

    public class CrudRequestEntityConfigBuilder<TRequest, TEntity>
        : ICrudRequestEntityConfigBuilder
        where TEntity : class
    {
        private readonly Dictionary<SelectorType, ISelector> _selectors
            = new Dictionary<SelectorType, ISelector>();

        private readonly Dictionary<ActionType, List<Func<TRequest, Task>>> _preActions
            = new Dictionary<ActionType, List<Func<TRequest, Task>>>();

        private readonly Dictionary<ActionType, List<Func<TEntity, Task>>> _postActions
            = new Dictionary<ActionType, List<Func<TEntity, Task>>>();

        private CrudOptionsConfig _optionsConfig = null;
        private TEntity _defaultValue = null;
        private Func<TRequest, Task<TEntity>> _createEntityFromRequest = null;
        private Func<TRequest, TEntity, Task> _updateEntityFromRequest = null;
        private Func<ICrudErrorHandler> _errorHandlerFactory = null;

        public CrudRequestEntityConfigBuilder()
        {
            foreach (var type in (ActionType[]) Enum.GetValues(typeof(ActionType)))
            {
                _preActions[type] = new List<Func<TRequest, Task>>();
                _postActions[type] = new List<Func<TEntity, Task>>();
            }
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> ConfigureOptions(Action<CrudOptionsConfig> config)
        {
            if (config == null)
            {
                _optionsConfig = null;
            }
            else
            {
                _optionsConfig = new CrudOptionsConfig();
                config(_optionsConfig);
            }

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UseErrorHandlerFactory(Func<ICrudErrorHandler> handlerFactory)
        {
            _errorHandlerFactory = handlerFactory;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeCreating(Func<TRequest, Task> action) 
            => AddPreAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeCreating(Action<TRequest> action)
            => AddPreAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterCreating(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterCreating(Action<TEntity> action)
            => AddPostAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeUpdating(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeUpdating(Action<TRequest> action)
            => AddPreAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterUpdating(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterUpdating(Action<TEntity> action)
            => AddPostAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeDeleting(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeDeleting(Action<TRequest> action)
            => AddPreAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterDeleting(Func<TEntity, Task> action) 
            => AddPostAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterDeleting(Action<TEntity> action)
            => AddPostAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeSaving(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeSaving(Action<TRequest> action)
            => AddPreAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterSaving(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterSaving(Action<TEntity> action)
            => AddPostAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UseDefault(TEntity defaultValue)
        {
            _defaultValue = defaultValue;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForGetWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            _selectors[SelectorType.Get] = Selector.From(build(builder));

            return this;
        }
        
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForUpdateWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            _selectors[SelectorType.Update] = Selector.From(build(builder));

            return this;
        }
        
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForDeleteWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            _selectors[SelectorType.Delete] = Selector.From(build(builder));

            return this;
        }
        
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForAnyWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            var sel = Selector.From(build(builder));

            foreach (var type in (SelectorType[])Enum.GetValues(typeof(SelectorType)))
                _selectors[type] = sel;

            return this;
        }
        
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateWith(
            Func<TRequest, Task<TEntity>> creator)
        {
            _createEntityFromRequest = creator;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateWith(
            Func<TRequest, TEntity> creator)
        {
            _createEntityFromRequest = request => Task.FromResult(creator(request));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateWith(
            Func<TRequest, TEntity, Task> updator)
        {
            _updateEntityFromRequest = updator;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateWith(
            Action<TRequest, TEntity> updator)
        {
            _updateEntityFromRequest = (request, entity) =>
            {
                updator(request, entity);
                return Task.CompletedTask;
            };

            return this;
        }

        public void Build<TCompatibleRequest>(CrudRequestConfig<TCompatibleRequest> config)
        {
            if (_optionsConfig != null)
                config.SetOptionsFor<TEntity>(_optionsConfig);

            if (_errorHandlerFactory != null)
                config.ErrorConfig.SetErrorHandlerFor(typeof(TEntity), _errorHandlerFactory);

            config.SetDefault(_defaultValue);

            foreach (var (type, selector) in _selectors)
                config.SetEntitySelectorFor<TEntity>(type, selector);

            if (_createEntityFromRequest != null)
                config.SetEntityCreator(request => _createEntityFromRequest((TRequest)request));

            if (_updateEntityFromRequest != null)
                config.SetEntityUpdator<TEntity>((request, entity) => _updateEntityFromRequest((TRequest)request, entity));

            foreach (var type in (ActionType[]) Enum.GetValues(typeof(ActionType)))
            {
                Func<object, Task> ConvertAction<T>(Func<T, Task> action)
                    => new Func<object, Task>(x => action((T) x));

                var preActions = _preActions[type];
                if (preActions.Count > 0)
                {
                    config.SetPreActionsFor<TEntity>(type, 
                        new ActionList(preActions.Select(ConvertAction<TRequest>)));
                }

                var postActions = _postActions[type];
                if (postActions.Count > 0)
                {
                    config.SetPostActionsFor<TEntity>(type,
                        new ActionList(postActions.Select(ConvertAction<TEntity>)));
                }
            }
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPreAction(ActionType type, Func<TRequest, Task> action)
        {
            if (action != null)
                _preActions[type].Add(action);

            return this;
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPreAction(ActionType type, Action<TRequest> action)
        {
            if (action != null)
                _preActions[type].Add(request =>
                {
                    action(request);
                    return Task.CompletedTask;
                });

            return this;
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPostAction(ActionType type, Func<TEntity, Task> action)
        {
            if (action != null)
                _postActions[type].Add(action);

            return this;
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPostAction(ActionType type, Action<TEntity> action)
        {
            if (action != null)
                _postActions[type].Add(request =>
                {
                    action(request);
                    return Task.CompletedTask;
                });

            return this;
        }
    }
}
