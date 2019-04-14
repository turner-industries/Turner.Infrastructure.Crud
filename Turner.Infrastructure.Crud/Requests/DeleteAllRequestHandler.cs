using System;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class DeleteAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : IDeleteAllRequest
    {
        protected readonly RequestOptions Options;

        protected DeleteAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> DeleteEntities(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var entities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .ToArrayAsync(ct)
                .Configure();

            ct.ThrowIfCancellationRequested();

            entities = await Context.Set<TEntity>().DeleteAsync(entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunEntityHooks<TEntity>(RequestConfig, entities, ct).Configure();

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
    }

    internal class DeleteAllRequestHandler<TRequest, TEntity>
        : DeleteAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IDeleteAllRequest<TEntity>
    {
        public DeleteAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    await DeleteEntities(request, ct).Configure();
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(RequestFailedError.From(request, e));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(RequestCanceledError.From(request, e));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(HookFailedError.From(request, e));
                }
            }

            return Response.Success();
        }
    }

    internal class DeleteAllRequestHandler<TRequest, TEntity, TOut>
        : DeleteAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, DeleteAllResult<TOut>>
        where TEntity : class
        where TRequest : IDeleteAllRequest<TEntity, TOut>
    {
        public DeleteAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<DeleteAllResult<TOut>>> HandleAsync(TRequest request)
        {
            DeleteAllResult<TOut> result = null;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    var entities = await DeleteEntities(request, ct).Configure();
                    var tOuts = await entities.CreateResults<TEntity, TOut>(RequestConfig, ct).Configure();
                    var items = await request.RunResultHooks(RequestConfig, tOuts, ct).Configure();

                    result = new DeleteAllResult<TOut>(items);
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<DeleteAllResult<TOut>>(RequestFailedError.From(request, e, result));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<DeleteAllResult<TOut>>(RequestCanceledError.From(request, e, result));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<DeleteAllResult<TOut>>(HookFailedError.From(request, e, result));
                }
            }

            return result.AsResponse();
        }
    }
}
