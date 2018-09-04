using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IDeleteAlgorithm
    {
        DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class;

        Task<TEntity> DeleteEntityAsync<TEntity>(DbContext context, TEntity entity)
            where TEntity : class;

        Task SaveChangesAsync(DbContext context);
    }

    public class StandardDeleteAlgorithm : IDeleteAlgorithm
    {
        private readonly IContextAccess _contextAccess;
        private readonly IDbSetAccess _setAccess;

        public StandardDeleteAlgorithm(IContextAccess contextAccess, 
            IDbSetAccess setAccess)
        {
            _contextAccess = contextAccess;
            _setAccess = setAccess;
        }

        public DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class
        {
            return _contextAccess.GetEntities<TEntity>(context);
        }

        public Task<TEntity> DeleteEntityAsync<TEntity>(DbContext context, TEntity entity)
            where TEntity : class
        {
            var set = _contextAccess.GetEntities<TEntity>(context);
            return _setAccess.DeleteAsync(entity, set);
        }

        public Task SaveChangesAsync(DbContext context)
        {
            return _contextAccess.ApplyChangesAsync(context);
        }
    }

    internal abstract class DeleteRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly IDeleteAlgorithm Algorithm;

        public DeleteRequestHandlerBase(DbContext context, 
            CrudConfigManager profileManager,
            IDeleteAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
        }

        protected async Task<TEntity> GetEntity(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>(SelectorType.Delete);
            var entity = await Algorithm.GetEntities<TEntity>(Context)
                .SelectSingleAsync(request, selector)
                .Configure();

            return entity;
        }

        protected async Task DeleteEntity(TRequest request, TEntity entity)
        {
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Delete, request).Configure();
            await Algorithm.DeleteEntityAsync(Context, entity).Configure();
            await RequestConfig.RunPostActionsFor(ActionType.Delete, request, entity).Configure();

            await Algorithm.SaveChangesAsync(Context).Configure();
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IDeleteRequest<TEntity>
    {
        public DeleteRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            IDeleteAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            var entity = default(TEntity);

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch(error);
            }

            if (entity == null && RequestConfig.ErrorConfig.FailedToFindInDeleteIsError)
                return ErrorDispatcher.Dispatch(new FailedToFindError(request, typeof(TEntity)));

            if (entity != null)
                await DeleteEntity(request, entity).Configure();

            return Response.Success();
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity, TOut>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IDeleteRequest<TEntity, TOut>
    {
        public DeleteRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            IDeleteAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var entity = default(TEntity);
            var result = default(TOut);

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            if (entity == null && RequestConfig.ErrorConfig.FailedToFindInDeleteIsError)
            {
                var error = new FailedToFindError(request, typeof(TEntity));
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            if (entity != null)
            {
                await DeleteEntity(request, entity).Configure();
                result = Mapper.Map<TOut>(entity);
            }

            return result.AsResponse();
        }
    }
}
