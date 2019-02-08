using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class MergeRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly RequestOptions Options;

        protected MergeRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> MergeEntities(TRequest request)
        {
            var requestHooks = RequestConfig.GetRequestHooks(request);
            foreach (var hook in requestHooks)
                await hook.Run(request).Configure();
            
            var entities = await GetEntities(request).Configure();

            var data = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)data.ItemSource(request)).ToArray();

            var itemHooks = RequestConfig.GetItemHooksFor<TEntity>(request);
            foreach (var item in items)
                foreach (var hook in itemHooks)
                    await hook.Run(request, item).Configure();

            var joinedItems = RequestConfig
                .Join(items.Where(x => x != null), entities)
                .ToArray();

            var createdEntities = await CreateEntities(
                joinedItems.Length, joinedItems.Where(x => x.Item2 == null));

            var updatedEntities = await UpdateEntities(
                joinedItems.Length, joinedItems.Where(x => x.Item2 != null));

            var changedEntities = updatedEntities.Concat(createdEntities).ToArray();

            var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>(request);
            foreach (var entity in changedEntities)
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity).Configure();

            await Context.ApplyChangesAsync().Configure();

            return changedEntities;
        }

        private async Task<TEntity[]> GetEntities(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var entities = Context.EntitySet<TEntity>().AsQueryable();

            entities = entities.Where(selector(request));
            entities = RequestConfig
                .GetFiltersFor<TEntity>()
                .Aggregate(entities, (current, filter) => filter.Filter(request, current));

            return await Context.ToArrayAsync(entities).Configure();
        }

        private async Task<TEntity[]> CreateEntities(int estimatedCount, IEnumerable<Tuple<object, TEntity>> items)
        {
            var creator = RequestConfig.GetCreatorFor<TEntity>();

            var createdEntities = new List<TEntity>(estimatedCount);
            foreach (var item in items)
                createdEntities.Add(await creator(item.Item1).Configure());
            
            var entities = await Context.EntitySet<TEntity>().CreateAsync(createdEntities).Configure();
            
            return entities;
        }

        private async Task<TEntity[]> UpdateEntities(int estimatedCount, IEnumerable<Tuple<object, TEntity>> items)
        {
            var updator = RequestConfig.GetUpdatorFor<TEntity>();

            var updatedEntities = new List<TEntity>(estimatedCount);
            foreach (var item in items)
                updatedEntities.Add(await updator(item.Item1, item.Item2).Configure());
            
            var entities = await Context.EntitySet<TEntity>().UpdateAsync(updatedEntities).Configure();
            
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
            await MergeEntities(request).Configure();

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
            var entities = await MergeEntities(request).Configure();

            var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
            var items = new List<TOut>(await Task.WhenAll(entities.Select(transform)).Configure());

            var result = new MergeResult<TOut>(items);

            return result.AsResponse();
        }
    }
}
