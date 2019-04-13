using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class UpdateAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly RequestOptions Options;

        protected UpdateAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }
        
        protected async Task<TEntity[]> UpdateEntities(TRequest request, CancellationToken ct)
        {
            var requestHooks = RequestConfig.GetRequestHooks();
            foreach (var hook in requestHooks)
                await hook.Run(request, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var entities = await GetEntities(request, ct).Configure();
            ct.ThrowIfCancellationRequested();

            var data = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)data.ItemSource(request)).ToArray();

            var itemHooks = RequestConfig.GetItemHooksFor<TEntity>();
            foreach (var hook in itemHooks)
                for (var i = 0; i < items.Length; ++i)
                    items[i] = await hook.Run(request, items[i], ct).Configure();

            ct.ThrowIfCancellationRequested();

            var updator = RequestConfig.GetUpdatorFor<TEntity>();

            var updatedEntities = new List<TEntity>(entities.Length);
            foreach (var item in RequestConfig.Join(items, entities))
            {
                if (item.Item1 == null || item.Item2 == null)
                    continue;

                updatedEntities.Add(await updator(request, item.Item1, item.Item2, ct).Configure());
                ct.ThrowIfCancellationRequested();
            }

            entities = await Context.Set<TEntity>().UpdateAsync(updatedEntities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>();
            foreach (var entity in updatedEntities)
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity, ct).Configure();

            ct.ThrowIfCancellationRequested();

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }

        private async Task<TEntity[]> GetEntities(TRequest request, CancellationToken ct)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var entities = Context.Set<TEntity>().AsQueryable();
            
            entities = entities.Where(selector(request));
            entities = RequestConfig
                .GetFiltersFor<TEntity>()
                .Aggregate(entities, (current, filter) => (IQueryable<TEntity>)filter.Filter(request, current));

            return await entities.ToArrayAsync(ct).Configure();
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
                await UpdateEntities(request, cts.Token).Configure();

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
            UpdateAllResult<TOut> result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                var entities = await UpdateEntities(request, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                var items = new List<TOut>(await Task.WhenAll(entities.Select(x => transform(x, ct))));
                ct.ThrowIfCancellationRequested();

                var resultHooks = RequestConfig.GetResultHooks();
                foreach (var hook in resultHooks)
                    for (var i = 0; i < items.Count; ++i)
                        items[i] = (TOut)await hook.Run(request, items[i], ct).Configure();

                ct.ThrowIfCancellationRequested();

                result = new UpdateAllResult<TOut>(items);
            }

            return result.AsResponse();
        }
    }
}
