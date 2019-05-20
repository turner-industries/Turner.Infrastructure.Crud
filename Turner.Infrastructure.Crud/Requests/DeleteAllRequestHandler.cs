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
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var entities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .ToArrayAsync(ct)
                .Configure();

            ct.ThrowIfCancellationRequested();

            await request.RunEntityHooks<TEntity>(RequestConfig, entities, ct).Configure();

            entities = await Context.Set<TEntity>().DeleteAsync(DataContext, entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

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

        public Task<Response> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, (_, token) => (Task)DeleteEntities(request, token));
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

        public Task<Response<DeleteAllResult<TOut>>> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, HandleAsync);
        }

        private async Task<DeleteAllResult<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            var entities = await DeleteEntities(request, token).Configure();
            var items = await entities.CreateResults<TEntity, TOut>(RequestConfig, token).Configure();
            var result = new DeleteAllResult<TOut>(items);

            return await request.RunResultHooks(RequestConfig, result, token).Configure();
        }
    }
}
