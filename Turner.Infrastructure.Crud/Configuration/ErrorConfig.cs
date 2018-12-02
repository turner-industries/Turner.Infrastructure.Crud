using System;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration
{
    public class ErrorConfig
    {
        private Func<ICrudErrorHandler> _requestErrorHandlerFactory;

        private readonly Dictionary<Type, Func<ICrudErrorHandler>> _errorHandlerFactoryCache
            = new Dictionary<Type, Func<ICrudErrorHandler>>();

        private readonly Dictionary<Type, Func<ICrudErrorHandler>> _errorHandlerFactories
            = new Dictionary<Type, Func<ICrudErrorHandler>>();

        public bool FailedToFindInGetIsError { get; set; } = true;

        public bool FailedToFindInGetAllIsError { get; set; }

        public bool FailedToFindInUpdateIsError { get; set; } = true;

        public bool FailedToFindInDeleteIsError { get; set; }

        public ICrudErrorHandler GetErrorHandlerFor<TEntity>()
            where TEntity : class
        {
            if (_errorHandlerFactoryCache.TryGetValue(typeof(TEntity), out var cachedFactory))
                return cachedFactory?.Invoke();

            foreach (var tEntity in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_errorHandlerFactories.TryGetValue(tEntity, out var factory))
                {
                    _errorHandlerFactoryCache[typeof(TEntity)] = factory;
                    return factory();
                }
            }

            _errorHandlerFactoryCache[typeof(TEntity)] = _requestErrorHandlerFactory;

            return _requestErrorHandlerFactory?.Invoke();
        }

        internal void SetErrorHandler(Func<ICrudErrorHandler> factory)
        {
            _requestErrorHandlerFactory = factory;
        }

        internal void SetErrorHandlerFor(Type tEntity, Func<ICrudErrorHandler> factory)
        {
            _errorHandlerFactories[tEntity] = factory;
        }
    }
}