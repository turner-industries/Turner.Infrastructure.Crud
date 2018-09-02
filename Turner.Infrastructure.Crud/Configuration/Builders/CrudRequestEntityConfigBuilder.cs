﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private Func<TRequest, TEntity> _createEntityFromRequest;

        public CrudRequestEntityConfigBuilder()
        {
            _preCreateActions = new List<Func<TRequest, Task>>();
            _postCreateActions = new List<Func<TEntity, Task>>();
            _createEntityFromRequest = null;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeCreating(
            Func<TRequest, Task> preCreateAction)
        {
            if (preCreateAction != null)
                _preCreateActions.Add(preCreateAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterCreating(
            Func<TEntity, Task> postCreateAction)
        {
            if (postCreateAction != null)
                _postCreateActions.Add(postCreateAction);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateWith(
            Func<TRequest, TEntity> creator)
        {
            _createEntityFromRequest = creator;

            return this;
        }

        public void Build<TCompatibleRequest>(CrudRequestConfig<TCompatibleRequest> config)
        {
            if (_createEntityFromRequest != null)
                config.SetEntityCreator(request => _createEntityFromRequest((TRequest)request));

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
        }
    }
}
