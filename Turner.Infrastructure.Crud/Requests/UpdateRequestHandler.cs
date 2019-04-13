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
    internal abstract class UpdateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : IUpdateRequest
    {
        protected UpdateRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected Task<TEntity> GetEntity(TRequest request, CancellationToken ct)
        {
            return Context.Set<TEntity>()
                .SelectWith(request, RequestConfig.GetSelectorFor<TEntity>())
                .SingleOrDefaultAsync(ct);
        }

        protected async Task<TEntity> UpdateEntity(TRequest request, TEntity entity, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig.GetRequestHooks(), ct).Configure();

            var updator = RequestConfig.GetUpdatorFor<TEntity>();
            var data = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);
            entity = await updator(request, data, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            entity = await Context.Set<TEntity>().UpdateAsync(entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunEntityHooks(RequestConfig.GetEntityHooksFor<TEntity>(), entity, ct);

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

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
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                TEntity entity;

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

                if (entity == null && RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
                    return ErrorDispatcher.Dispatch(new FailedToFindError(request, typeof(TEntity)));

                if (entity != null)
                {
                    await UpdateEntity(request, entity, ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
            }

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
            var result = default(TOut);

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                TEntity entity;

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

                if (entity == null && RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
                {
                    var error = new FailedToFindError(request, typeof(TEntity));
                    return ErrorDispatcher.Dispatch<TOut>(error);
                }

                if (entity != null)
                {
                    entity = await UpdateEntity(request, entity, ct).Configure();
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
