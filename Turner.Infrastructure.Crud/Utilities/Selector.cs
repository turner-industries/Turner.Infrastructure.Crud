using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud
{
    public interface ISelector
    {
        void Bind<TRequest, TEntity>(Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class;

        Func<object, Expression<Func<TEntity, bool>>> Get<TEntity>()
                where TEntity : class;
    }

    public class Selector : ISelector
    {
        private Func<object, LambdaExpression> _selector;
        private Type _boundRequestType;
        private Type _boundEntityType;

        public static Selector From<TRequest, TEntity>(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class
        {
            var s = new Selector();
            s.Bind(selector);
            return s;
        }

        public void Bind<TRequest, TEntity>(
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class
        {
            _selector = request => selector((TRequest)request);

            _boundRequestType = typeof(TRequest);
            _boundEntityType = typeof(TEntity);
        }

        public Func<object, Expression<Func<TEntity, bool>>> Get<TEntity>() 
            where TEntity : class
        {
            return Select<TEntity>;
        }

        private Expression<Func<TEntity, bool>> Select<TEntity>(object request)
            where TEntity : class
        {
            if (request == null ||
                !_boundRequestType.IsAssignableFrom(request.GetType()))
            {
                var message =
                    $"Unable to create a selector for entity '{typeof(TEntity)}' " +
                    $"from a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{_boundRequestType}'.";

                throw new CrudRequestFailedException(message)
                {
                    RequestTypeProperty = request.GetType()
                };
            }

            var selectExpr = _selector(request);

            if (selectExpr == null)
                return null;

            if (typeof(TEntity) == _boundEntityType)
                return (Expression<Func<TEntity, bool>>)selectExpr;

            var entityParam = Expression.Parameter(typeof(TEntity));
            var tParam = Expression.Convert(entityParam, _boundEntityType);
            var invocation = Expression.Invoke(selectExpr, tParam);

            return Expression.Lambda<Func<TEntity, bool>>(invocation, entityParam);
        }
    }

    public static class SelectorExtensions
    {
        public static TEntity Select<TRequest, TEntity>(
            this IQueryable<TEntity> entities, TRequest request, ISelector selector)
            where TEntity : class
        {
            return entities.SingleOrDefault(selector.Get<TEntity>()(request));
        }

        public static Task<TEntity> SelectAsync<TRequest, TEntity>(
            this IQueryable<TEntity> entities, TRequest request, ISelector selector)
            where TEntity : class
        {
            return entities.SingleOrDefaultAsync(selector.Get<TEntity>()(request));
        }
    }
}
