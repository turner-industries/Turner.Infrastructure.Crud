using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
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
            await request.RunRequestHooks(RequestConfig.GetRequestHooks(), ct).Configure();

            ct.ThrowIfCancellationRequested();

            var entities = await GetEntities(request, ct);
            ct.ThrowIfCancellationRequested();

            entities = await Context.Set<TEntity>().DeleteAsync(entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunEntityHooks(RequestConfig.GetEntityHooksFor<TEntity>(), entities, ct);

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
        
        private Task<TEntity[]> GetEntities(TRequest request, CancellationToken ct)
        {
            return Context.Set<TEntity>()
                .FilterWith(request, RequestConfig.GetFiltersFor<TEntity>())
                .ToArrayAsync(ct);
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
                
                var entities = await DeleteEntities(request, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                var items = new List<TOut>(await Task.WhenAll(entities.Select(x => transform(x, ct))).Configure());
                ct.ThrowIfCancellationRequested();

                items = await request.RunResultHooks(RequestConfig.GetResultHooks(), items, ct);

                result = new DeleteAllResult<TOut>(items);
            }

            return result.AsResponse();
        }
    }
}
