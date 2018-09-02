using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestConfig
    {
        TEntity CreateEntity<TEntity>(object request)
            where TEntity : class;

        Task PreCreate<TEntity>(object request)
            where TEntity : class;

        Task PostCreate<TEntity>(TEntity entity)
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private readonly Dictionary<Type, Func<object, object>> _entityCreators
            = new Dictionary<Type, Func<object, object>>();
     
        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPreCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPostCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        internal void SetEntityCreator<TEntity>(
            Func<object, TEntity> creator)
            where TEntity : class
        {
            _entityCreators[typeof(TEntity)] = request => creator(request);
        }

        internal void SetPreCreateActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPreCreateActions[typeof(TEntity)] = actions;
        }

        internal void SetPostCreateActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPostCreateActions[typeof(TEntity)] = actions;
        }
        
        public TEntity CreateEntity<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message = 
                    $"Unable to create an entity of type '{typeof(TEntity)}' " +
                    $"from a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'."; 

                throw new BadCrudConfigurationException(message);
            }

            if (_entityCreators.TryGetValue(typeof(TEntity), out var creator))
                return (TEntity) creator(request);
            
            return Mapper.Map<TEntity>(request);
        }

        public async Task PreCreate<TEntity>(object request) 
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to run PreCreate actions on a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityPreCreateActions.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(request);
                }
            }
        }

        public async Task PostCreate<TEntity>(TEntity entity) 
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityPostCreateActions.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(entity);
                }
            }
        }

        private void BuildEntityStack(Type tEntity, ref List<Type> entities)
        {
            var entityParents = new[] { tEntity.BaseType }
                .Concat(tEntity.GetInterfaces())
                .Where(x => x != null);

            foreach (var parent in entityParents)
                BuildEntityStack(parent, ref entities);

            entities.Add(tEntity);
        }
    }
}
