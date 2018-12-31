using AutoMapper;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class DeleteRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected DeleteRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected Task<TEntity> GetEntity(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var set = Context.EntitySet<TEntity>();

            return Context.SingleOrDefaultAsync(set, selector(request));
        }

        protected async Task<TEntity> DeleteEntity(TRequest request, TEntity entity)
        {
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Delete, request).Configure();
            entity = await Context.EntitySet<TEntity>().DeleteAsync(entity).Configure();
            await RequestConfig.RunPostActionsFor(ActionType.Delete, request, entity).Configure();

            await Context.ApplyChangesAsync().Configure();

            return entity;
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IDeleteRequest<TEntity>
    {
        public DeleteRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            TEntity entity;

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
        public DeleteRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            TEntity entity;
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
                entity = await DeleteEntity(request, entity).Configure();
                result = Mapper.Map<TOut>(entity);
            }

            return result.AsResponse();
        }
    }
}
