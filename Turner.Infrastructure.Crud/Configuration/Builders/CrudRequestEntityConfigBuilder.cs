using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
        private readonly List<Func<TRequest, Task>> _preCreateActions;
        private readonly List<Func<TEntity, Task>> _postCreateActions;
        private readonly List<Func<TRequest, Task>> _preUpdateActions;
        private readonly List<Func<TEntity, Task>> _postUpdateActions;
        private readonly List<Func<TRequest, Task>> _preDeleteActions;
        private readonly List<Func<TEntity, Task>> _postDeleteActions;

        private TEntity _defaultValue;
        private ISelector _selectEntityFromRequestForGet;
        private ISelector _selectEntityFromRequestForUpdate;
        private ISelector _selectEntityFromRequestForDelete;
        private Func<TRequest, Task<TEntity>> _createEntityFromRequest;
        private Func<TRequest, TEntity, Task> _updateEntityFromRequest;

        public CrudRequestEntityConfigBuilder()
        {
            _preCreateActions = new List<Func<TRequest, Task>>();
            _postCreateActions = new List<Func<TEntity, Task>>();
            _preUpdateActions = new List<Func<TRequest, Task>>();
            _postUpdateActions = new List<Func<TEntity, Task>>();
            _preDeleteActions = new List<Func<TRequest, Task>>();
            _postDeleteActions = new List<Func<TEntity, Task>>();

            _defaultValue = null;
            _selectEntityFromRequestForGet = null;
            _selectEntityFromRequestForUpdate = null;
            _selectEntityFromRequestForDelete = null;
            _createEntityFromRequest = null;
            _updateEntityFromRequest = null;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeCreating(
            Func<TRequest, Task> preCreateAction)
        {
            if (preCreateAction != null)
                _preCreateActions.Add(preCreateAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeCreating(
            Action<TRequest> preCreateAction)
        {
            if (preCreateAction != null)
                _preCreateActions.Add(request => 
                {
                    preCreateAction(request);
                    return Task.CompletedTask;
                });

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterCreating(
            Func<TEntity, Task> postCreateAction)
        {
            if (postCreateAction != null)
                _postCreateActions.Add(postCreateAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterCreating(
            Action<TEntity> postCreateAction)
        {
            if (postCreateAction != null)
                _postCreateActions.Add(entity =>
                {
                    postCreateAction(entity);
                    return Task.CompletedTask;
                });

            return this;
        }
        
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeUpdating(
            Func<TRequest, Task> preUpdateAction)
        {
            if (preUpdateAction != null)
                _preUpdateActions.Add(preUpdateAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeUpdating(
            Action<TRequest> preUpdateAction)
        {
            if (preUpdateAction != null)
                _preUpdateActions.Add(request =>
                {
                    preUpdateAction(request);
                    return Task.CompletedTask;
                });

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterUpdating(
            Func<TEntity, Task> postUpdateAction)
        {
            if (postUpdateAction != null)
                _postUpdateActions.Add(postUpdateAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterUpdating(
            Action<TEntity> postUpdateAction)
        {
            if (postUpdateAction != null)
                _postUpdateActions.Add(entity =>
                {
                    postUpdateAction(entity);
                    return Task.CompletedTask;
                });

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeDeleting(
            Func<TRequest, Task> preDeleteAction)
        {
            if (preDeleteAction != null)
                _preDeleteActions.Add(preDeleteAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeDeleting(
            Action<TRequest> preDeleteAction)
        {
            if (preDeleteAction != null)
                _preDeleteActions.Add(request =>
                {
                    preDeleteAction(request);
                    return Task.CompletedTask;
                });

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterDeleting(
            Func<TEntity, Task> postDeleteAction)
        {
            if (postDeleteAction != null)
                _postDeleteActions.Add(postDeleteAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterDeleting(
            Action<TEntity> postDeleteAction)
        {
            if (postDeleteAction != null)
                _postDeleteActions.Add(entity =>
                {
                    postDeleteAction(entity);
                    return Task.CompletedTask;
                });

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UseDefault(
            TEntity defaultValue)
        {
            _defaultValue = defaultValue;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForGetWith(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
        {
            _selectEntityFromRequestForGet = Selector.From(selector);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForGetWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            _selectEntityFromRequestForGet = Selector.From(build(builder));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForUpdateWith(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
        {
            _selectEntityFromRequestForUpdate = Selector.From(selector);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForUpdateWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            _selectEntityFromRequestForUpdate = Selector.From(build(builder));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForDeleteWith(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
        {
            _selectEntityFromRequestForDelete = Selector.From(selector);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForDeleteWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            _selectEntityFromRequestForDelete = Selector.From(build(builder));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForAnyWith(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
        {
            _selectEntityFromRequestForGet = 
            _selectEntityFromRequestForUpdate = 
            _selectEntityFromRequestForDelete = Selector.From(selector);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectForAnyWith(
            Func<SelectorBuilder<TRequest, TEntity>, Func<TRequest, Expression<Func<TEntity, bool>>>> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();

            _selectEntityFromRequestForGet =
            _selectEntityFromRequestForUpdate =
            _selectEntityFromRequestForDelete = Selector.From(build(builder));

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
            config.SetDefault(_defaultValue);

            if (_selectEntityFromRequestForGet != null)
                config.SetEntitySelectorForGet<TEntity>(_selectEntityFromRequestForGet);

            if (_selectEntityFromRequestForUpdate != null)
                config.SetEntitySelectorForUpdate<TEntity>(_selectEntityFromRequestForUpdate);

            if (_selectEntityFromRequestForDelete != null)
                config.SetEntitySelectorForDelete<TEntity>(_selectEntityFromRequestForDelete);

            if (_createEntityFromRequest != null)
                config.SetEntityCreator(request => _createEntityFromRequest((TRequest)request));

            if (_updateEntityFromRequest != null)
                config.SetEntityUpdator<TEntity>((request, entity) => _updateEntityFromRequest((TRequest)request, entity));

            if (_preCreateActions.Count > 0)
            {
                var actions = _preCreateActions
                    .Select(action => new Func<object, Task>(x => action((TRequest) x)))
                    .ToList();

                config.SetPreCreateActions<TEntity>(actions);
            }

            if (_postCreateActions.Count > 0)
            {
                var actions = _postCreateActions
                    .Select(action => new Func<object, Task>(x => action((TEntity) x)))
                    .ToList();

                config.SetPostCreateActions<TEntity>(actions);
            }

            if (_preUpdateActions.Count > 0)
            {
                var actions = _preUpdateActions
                    .Select(action => new Func<object, Task>(x => action((TRequest) x)))
                    .ToList();

                config.SetPreUpdateActions<TEntity>(actions);
            }

            if (_postUpdateActions.Count > 0)
            {
                var actions = _postUpdateActions
                    .Select(action => new Func<object, Task>(x => action((TEntity) x)))
                    .ToList();

                config.SetPostUpdateActions<TEntity>(actions);
            }

            if (_preDeleteActions.Count > 0)
            {
                var actions = _preDeleteActions
                    .Select(action => new Func<object, Task>(x => action((TRequest)x)))
                    .ToList();

                config.SetPreDeleteActions<TEntity>(actions);
            }

            if (_postDeleteActions.Count > 0)
            {
                var actions = _postDeleteActions
                    .Select(action => new Func<object, Task>(x => action((TEntity)x)))
                    .ToList();

                config.SetPostDeleteActions<TEntity>(actions);
            }
        }
    }
}
