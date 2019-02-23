using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class DeleteAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly RequestOptions Options;

        protected DeleteAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> DeleteEntities(TRequest request, CancellationToken ct)
        {
            var requestHooks = RequestConfig.GetRequestHooks();
            var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>();

            foreach (var hook in requestHooks)
                await hook.Run(request, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var entities = await GetEntities(request, ct);
            ct.ThrowIfCancellationRequested();

            entities = await Context.EntitySet<TEntity>().DeleteAsync(entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            foreach (var entity in entities)
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity, ct).Configure();

            ct.ThrowIfCancellationRequested();

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
        
        private async Task<TEntity[]> GetEntities(TRequest request, CancellationToken ct)
        {
            var entities = Context.EntitySet<TEntity>().AsQueryable();

            foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                entities = filter.Filter(request, entities).Cast<TEntity>();

            return await Context.ToArrayAsync(entities, ct).Configure();
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
                await DeleteEntities(request, cts.Token).Configure();
            
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
            DeleteAllResult<TOut> result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                var resultHooks = RequestConfig.GetResultHooks();

                var entities = await DeleteEntities(request, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var items = new List<TOut>(await Task.WhenAll(entities.Select(x => transform(x, ct))).Configure());
                ct.ThrowIfCancellationRequested();

                foreach (var hook in resultHooks)
                    for (var i = 0; i < items.Count; ++i)
                        items[i] = (TOut)await hook.Run(request, items[i], ct).Configure();

                ct.ThrowIfCancellationRequested();

                result = new DeleteAllResult<TOut>(items);
            }

            return result.AsResponse();
        }
    }
}
