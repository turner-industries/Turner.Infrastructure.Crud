using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class UpdateAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : IUpdateAllRequest
    {
        protected readonly RequestOptions Options;

        protected UpdateAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }
        
        protected async Task<TEntity[]> UpdateEntities(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var itemSource = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)itemSource.ItemSource(request)).ToArray();

            items = await request.RunItemHooks<TEntity>(RequestConfig, items, ct).Configure();

            var entities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SelectWith(request, RequestConfig)
                .ToArrayAsync(ct)
                .Configure();

            ct.ThrowIfCancellationRequested();

            var joinedItems = RequestConfig.Join(items, entities).Where(x => x.Item2 != null);

            var updatedEntities = await request.UpdateEntities(RequestConfig, joinedItems, ct).Configure();

            entities = await Context.Set<TEntity>().UpdateAsync(updatedEntities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunEntityHooks<TEntity>(RequestConfig, entities, ct).Configure();
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
    }

    internal class UpdateAllRequestHandler<TRequest, TEntity>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IUpdateAllRequest<TEntity>
    {
        public UpdateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
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
                    await UpdateEntities(request, ct).Configure();
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

    internal class UpdateAllRequestHandler<TRequest, TEntity, TOut>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, UpdateAllResult<TOut>>
        where TEntity : class
        where TRequest : IUpdateAllRequest<TEntity, TOut>
    {
        public UpdateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<UpdateAllResult<TOut>>> HandleAsync(TRequest request)
        {
            UpdateAllResult<TOut> result = null;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    var entities = await UpdateEntities(request, ct).Configure();
                    var tOuts = await entities.CreateResults<TEntity, TOut>(RequestConfig, ct).Configure();
                    var items = await request.RunResultHooks(RequestConfig, tOuts, ct).Configure();

                    result = new UpdateAllResult<TOut>(items);
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<UpdateAllResult<TOut>>(RequestFailedError.From(request, e, result));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<UpdateAllResult<TOut>>(RequestCanceledError.From(request, e, result));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<UpdateAllResult<TOut>>(HookFailedError.From(request, e, result));
                }
            }

            return result.AsResponse();
        }
    }
}
