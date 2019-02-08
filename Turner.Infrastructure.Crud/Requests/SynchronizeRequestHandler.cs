using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class SynchronizeRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly RequestOptions Options;

        protected SynchronizeRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> SynchronizeEntities(TRequest request)
        {
            var requestHooks = RequestConfig.GetRequestHooks(request);
            foreach (var hook in requestHooks)
                await hook.Run(request).Configure();
            
            await DeleteEntities(request).Configure();

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

        private Task DeleteEntities(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var entities = Context.EntitySet<TEntity>().AsQueryable();
            var where = selector(request);
            var notWhere = where.Update(
                Expression.NotEqual(where.Body, Expression.Constant(true)), 
                where.Parameters);

            entities = entities.Where(notWhere);

            return Context.EntitySet<TEntity>().DeleteAsync(entities);
        }

        private async Task<TEntity[]> GetEntities(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var entities = Context.EntitySet<TEntity>().AsQueryable();
            
            entities = entities.Where(selector(request));

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
            await SynchronizeEntities(request).Configure();

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
            var entities = await SynchronizeEntities(request).Configure();
            var result = new SynchronizeResult<TOut>(Mapper.Map<List<TOut>>(entities));

            return result.AsResponse();
        }
    }
}
