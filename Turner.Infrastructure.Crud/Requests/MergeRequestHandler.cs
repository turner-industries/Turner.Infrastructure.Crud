using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class MergeRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : IMergeRequest
    {
        protected readonly RequestOptions Options;

        protected MergeRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> MergeEntities(TRequest request, CancellationToken ct)
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

            var joinedItems = RequestConfig
                .Join(items.Where(x => x != null), entities)
                .ToArray();

            var createdEntities = await CreateEntities(request, 
                joinedItems.Where(x => x.Item2 == null).Select(x => x.Item1), ct).Configure();

            ct.ThrowIfCancellationRequested();

            var updatedEntities = await UpdateEntities(request, 
                joinedItems.Where(x => x.Item2 != null), ct).Configure();

            ct.ThrowIfCancellationRequested();

            var mergedEntities = updatedEntities.Concat(createdEntities).ToArray();

            await request.RunEntityHooks<TEntity>(RequestConfig, mergedEntities, ct).Configure();

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return mergedEntities;
        }

        private async Task<TEntity[]> CreateEntities(TRequest request, 
            IEnumerable<object> items, 
            CancellationToken ct)
        {
            var entities = await request.CreateEntities<TEntity>(RequestConfig, items, ct).Configure();

            entities = await Context.Set<TEntity>().CreateAsync(entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }

        private async Task<TEntity[]> UpdateEntities(TRequest request, 
            IEnumerable<Tuple<object, TEntity>> items,
            CancellationToken ct)
        {
            var entities = await request.UpdateEntities(RequestConfig, items, ct).Configure();

            entities = await Context.Set<TEntity>().UpdateAsync(entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
    }

    internal class MergeRequestHandler<TRequest, TEntity>
        : MergeRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IMergeRequest<TEntity>
    {
        public MergeRequestHandler(IEntityContext context, CrudConfigManager profileManager)
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
                    await MergeEntities(request, ct).Configure();
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

    internal class MergeRequestHandler<TRequest, TEntity, TOut>
        : MergeRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, MergeResult<TOut>>
        where TEntity : class
        where TRequest : IMergeRequest<TEntity, TOut>
    {
        public MergeRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<MergeResult<TOut>>> HandleAsync(TRequest request)
        {
            MergeResult<TOut> result = null;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    var entities = await MergeEntities(request, ct).Configure();
                    var tOuts = await entities.CreateResults<TEntity, TOut>(RequestConfig, ct).Configure();
                    var items = await request.RunResultHooks(RequestConfig, tOuts, ct).Configure();

                    result = new MergeResult<TOut>(items);
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<MergeResult<TOut>>(RequestFailedError.From(request, e, result));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<MergeResult<TOut>>(RequestCanceledError.From(request, e, result));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<MergeResult<TOut>>(HookFailedError.From(request, e, result));
                }
            }

            return result.AsResponse();
        }
    }
}
