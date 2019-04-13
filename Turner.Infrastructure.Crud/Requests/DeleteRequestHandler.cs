using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class DeleteRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : IDeleteRequest
    {
        protected DeleteRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected Task<TEntity> GetEntity(TRequest request, CancellationToken ct)
        {
            return Context.Set<TEntity>()
                .SelectWith(request, RequestConfig.GetSelectorFor<TEntity>())
                .SingleOrDefaultAsync(ct);
        }

        protected async Task<TEntity> DeleteEntity(TRequest request, TEntity entity, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig.GetRequestHooks(), ct).Configure();

            ct.ThrowIfCancellationRequested();

            entity = await Context.Set<TEntity>().DeleteAsync(entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunEntityHooks(RequestConfig.GetEntityHooksFor<TEntity>(), entity, ct);

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

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

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    entity = await GetEntity(request, ct).Configure();
                }
                catch (CrudRequestFailedException e)
                {
                    var error = new RequestFailedError(request, e);
                    return ErrorDispatcher.Dispatch(error);
                }

                ct.ThrowIfCancellationRequested();

                if (entity == null && RequestConfig.ErrorConfig.FailedToFindInDeleteIsError)
                    return ErrorDispatcher.Dispatch(new FailedToFindError(request, typeof(TEntity)));

                if (entity != null)
                {
                    await DeleteEntity(request, entity, ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
            }

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

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    entity = await GetEntity(request, ct).Configure();
                }
                catch (CrudRequestFailedException e)
                {
                    var error = new RequestFailedError(request, e);
                    return ErrorDispatcher.Dispatch<TOut>(error);
                }

                ct.ThrowIfCancellationRequested();

                if (entity == null && RequestConfig.ErrorConfig.FailedToFindInDeleteIsError)
                {
                    var error = new FailedToFindError(request, typeof(TEntity));
                    return ErrorDispatcher.Dispatch<TOut>(error);
                }

                if (entity != null)
                {
                    entity = await DeleteEntity(request, entity, ct).Configure();
                    ct.ThrowIfCancellationRequested();

                    var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                    result = await transform(entity, ct).Configure();
                    ct.ThrowIfCancellationRequested();

                    result = await request.RunResultHooks(RequestConfig.GetResultHooks(), result, ct);
                }
            }

            return result.AsResponse();
        }
    }
}
