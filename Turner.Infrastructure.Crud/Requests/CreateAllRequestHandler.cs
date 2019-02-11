using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class CreateAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected CreateAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity[]> CreateEntities(TRequest request, CancellationToken ct)
        {
            var requestHooks = RequestConfig.GetRequestHooks();
            foreach (var hook in requestHooks)
                await hook.Run(request, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var itemSource = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)itemSource.ItemSource(request)).ToArray();

            var itemHooks = RequestConfig.GetItemHooksFor<TEntity>();
            foreach (var hook in itemHooks)
                for (var i = 0; i < items.Length; ++i)
                    items[i] = await hook.Run(request, items[i], ct).Configure();

            ct.ThrowIfCancellationRequested();

            var creator = RequestConfig.GetCreatorFor<TEntity>();
            
            var newEntities = new List<TEntity>();
            foreach (var item in items)
            {
                newEntities.Add(await creator(request, item, ct).Configure());
                ct.ThrowIfCancellationRequested();
            }
            
            var entities = await Context.EntitySet<TEntity>().CreateAsync(newEntities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>();
            foreach (var entity in entities)
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity, ct).Configure();

            ct.ThrowIfCancellationRequested();

            await Context.ApplyChangesAsync(ct).Configure();

            ct.ThrowIfCancellationRequested();

            return entities;
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity>
    {
        public CreateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
                await CreateEntities(request, cts.Token).Configure();

            return Response.Success();
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity, TOut>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, CreateAllResult<TOut>>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity, TOut>
    {
        public CreateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<CreateAllResult<TOut>>> HandleAsync(TRequest request)
        {
            CreateAllResult<TOut> result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                var resultHooks = RequestConfig.GetResultHooks();

                var entities = await CreateEntities(request, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var items = new List<TOut>(await Task.WhenAll(entities.Select(x => transform(x, ct))).Configure());
                ct.ThrowIfCancellationRequested();

                foreach (var hook in resultHooks)
                    for (var i = 0; i < items.Count; ++i)
                        items[i] = (TOut)await hook.Run(request, items[i], ct).Configure();

                ct.ThrowIfCancellationRequested();

                result = new CreateAllResult<TOut>(items);
            }

            return result.AsResponse();
        }
    }
}
