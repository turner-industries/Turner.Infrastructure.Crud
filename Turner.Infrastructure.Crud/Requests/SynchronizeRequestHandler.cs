using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class SynchronizeRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : ISynchronizeRequest
    {
        protected readonly RequestOptions Options;

        protected SynchronizeRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> SynchronizeEntities(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig.GetRequestHooks(), ct).Configure();

            await DeleteEntities(request, ct).Configure();
            ct.ThrowIfCancellationRequested();

            var entities = await GetEntities(request, ct).Configure();
            ct.ThrowIfCancellationRequested();

            var data = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)data.ItemSource(request)).ToArray();

            items = await request.RunItemHooks(RequestConfig.GetItemHooksFor<TEntity>(), items, ct);

            var joinedItems = RequestConfig
                .Join(items.Where(x => x != null), entities)
                .ToArray();

            var createdEntities = await CreateEntities(
                request, joinedItems.Length, joinedItems.Where(x => x.Item2 == null), ct);
            ct.ThrowIfCancellationRequested();

            var updatedEntities = await UpdateEntities(
                request, joinedItems.Length, joinedItems.Where(x => x.Item2 != null), ct);
            ct.ThrowIfCancellationRequested();

            var changedEntities = updatedEntities.Concat(createdEntities).ToArray();
            await request.RunEntityHooks(RequestConfig.GetEntityHooksFor<TEntity>(), changedEntities, ct);

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return changedEntities;
        }

        private async Task DeleteEntities(TRequest request, CancellationToken ct)
        {
            var whereClause = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>()(request);
            var notWhereClause = whereClause.Update(
                Expression.NotEqual(whereClause.Body, Expression.Constant(true)), 
                whereClause.Parameters);

            var deleteEntities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig.GetFiltersFor<TEntity>())
                .Where(notWhereClause)
                .ToArrayAsync(ct);

            await Context.Set<TEntity>().DeleteAsync(deleteEntities, ct);
        }

        private Task<TEntity[]> GetEntities(TRequest request, CancellationToken ct)
        {
            return Context.Set<TEntity>()
                .SelectWith(request, RequestConfig.GetSelectorFor<TEntity>())
                .ToArrayAsync(ct);
        }

        private async Task<TEntity[]> CreateEntities(TRequest request, 
            int estimatedCount, 
            IEnumerable<Tuple<object, TEntity>> items, 
            CancellationToken ct)
        {
            var creator = RequestConfig.GetCreatorFor<TEntity>();

            var createdEntities = new List<TEntity>(estimatedCount);
            foreach (var item in items)
            {
                createdEntities.Add(await creator(request, item.Item1, ct).Configure());
                ct.ThrowIfCancellationRequested();
            }

            var entities = await Context.Set<TEntity>().CreateAsync(createdEntities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }

        private async Task<TEntity[]> UpdateEntities(TRequest request, 
            int estimatedCount, 
            IEnumerable<Tuple<object, TEntity>> items,
            CancellationToken ct)
        {
            var updator = RequestConfig.GetUpdatorFor<TEntity>();

            var updatedEntities = new List<TEntity>(estimatedCount);
            foreach (var item in items)
            {
                updatedEntities.Add(await updator(request, item.Item1, item.Item2, ct).Configure());
                ct.ThrowIfCancellationRequested();
            }

            var entities = await Context.Set<TEntity>().UpdateAsync(updatedEntities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
    }

    internal class SynchronizeRequestHandler<TRequest, TEntity>
        : SynchronizeRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ISynchronizeRequest<TEntity>
    {
        public SynchronizeRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
                await SynchronizeEntities(request, cts.Token).Configure();

            return Response.Success();
        }
    }

    internal class SynchronizeRequestHandler<TRequest, TEntity, TOut>
        : SynchronizeRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, SynchronizeResult<TOut>>
        where TEntity : class
        where TRequest : ISynchronizeRequest<TEntity, TOut>
    {
        public SynchronizeRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<SynchronizeResult<TOut>>> HandleAsync(TRequest request)
        {
            SynchronizeResult<TOut> result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                var entities = await SynchronizeEntities(request, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                var items = new List<TOut>(await Task.WhenAll(entities.Select(x => transform(x, ct))));
                ct.ThrowIfCancellationRequested();

                items = await request.RunResultHooks(RequestConfig.GetResultHooks(), items, ct);

                result = new SynchronizeResult<TOut>(items);
            }

            return result.AsResponse();
        }
    }
}
