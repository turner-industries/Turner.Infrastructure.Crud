using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class UpdateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected UpdateRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected Task<TEntity> GetEntity(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var set = Context.EntitySet<TEntity>();
            
            return Context.SingleOrDefaultAsync(set, selector(request));
        }

        protected async Task<TEntity> UpdateEntity(TRequest request, TEntity entity)
        {
            var updator = RequestConfig.GetUpdatorFor<TEntity>();
            var data = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            var requestHooks = RequestConfig.GetRequestHooks(request);
            foreach (var hook in requestHooks)
                await hook.Run(request).Configure();

            entity = await updator(request, data, entity).Configure();
            entity = await Context.EntitySet<TEntity>().UpdateAsync(entity).Configure();
            
            var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>(request);
            foreach (var hook in entityHooks)
                await hook.Run(request, entity).Configure();

            await Context.ApplyChangesAsync().Configure();

            return entity;
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity>
    {
        public UpdateRequestHandler(IEntityContext context, CrudConfigManager profileManager)
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

            if (entity == null && RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
                return ErrorDispatcher.Dispatch(new FailedToFindError(request, typeof(TEntity)));

            if (entity != null)
                await UpdateEntity(request, entity).Configure();

            return Response.Success();
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity, TOut>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity, TOut>
    {
        public UpdateRequestHandler(IEntityContext context, CrudConfigManager profileManager)
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
            
            if (entity == null && RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
            {
                var error = new FailedToFindError(request, typeof(TEntity));
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            if (entity != null)
            {
                entity = await UpdateEntity(request, entity).Configure();

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                result = await transform(entity).Configure();

                var resultHooks = RequestConfig.GetResultHooks(request);
                foreach (var hook in resultHooks)
                    result = (TOut)await hook.Run(request, result).Configure();
            }

            return result.AsResponse();
        }
    }
}
