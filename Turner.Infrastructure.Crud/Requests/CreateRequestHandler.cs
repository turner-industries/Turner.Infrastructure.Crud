using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class CreateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : ICreateRequest
    {
        protected CreateRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity> CreateEntity(TRequest request, 
            CancellationToken ct = default(CancellationToken))
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();
            
            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);
            var entity = await request.CreateEntity<TEntity>(RequestConfig, item, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

            entity = await Context.Set<TEntity>().CreateAsync(DataContext, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }
    }

    internal class CreateRequestHandler<TRequest, TEntity>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity>
    {
        public CreateRequestHandler(IEntityContext context, 
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, _ => (Task)CreateEntity(request));
        }
    }

    internal class CreateRequestHandler<TRequest, TEntity, TOut>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity, TOut>
    {
        public CreateRequestHandler(IEntityContext context, 
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response<TOut>> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, _HandleAsync);
        }

        private async Task<TOut> _HandleAsync(TRequest request)
        {
            var entity = await CreateEntity(request).Configure();
            var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig).Configure();
            var result = await request.RunResultHooks(RequestConfig, tOut).Configure();

            return result;
        }
    }
}
