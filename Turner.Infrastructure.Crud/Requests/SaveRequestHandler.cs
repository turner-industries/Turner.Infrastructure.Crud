using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class SaveRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : ISaveRequest
    {
        protected readonly RequestOptions Options;

        protected SaveRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }
        
        protected async Task<TEntity> SaveEntity(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();

            ct.ThrowIfCancellationRequested();

            if (entity == null)
            {
                entity = await CreateEntity(request, item, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            else
            {
                entity = await UpdateEntity(request, item, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }

        private async Task<TEntity> CreateEntity(TRequest request, object item, CancellationToken ct)
        {
            var entity = await request.CreateEntity<TEntity>(RequestConfig, item, ct);

            await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

            entity = await Context.Set<TEntity>().CreateAsync(DataContext, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }

        private async Task<TEntity> UpdateEntity(TRequest request, object item, TEntity entity, CancellationToken ct)
        {
            entity = await request.UpdateEntity(RequestConfig, item, entity, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

            entity = await Context.Set<TEntity>().UpdateAsync(DataContext, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }
    }

    internal class SaveRequestHandler<TRequest, TEntity>
        : SaveRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ISaveRequest<TEntity>
    {
        public SaveRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, (_, token) => (Task)SaveEntity(request, token));
        }
    }

    internal class SaveRequestHandler<TRequest, TEntity, TOut>
        : SaveRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : ISaveRequest<TEntity, TOut>
    {
        public SaveRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response<TOut>> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, HandleAsync);
        }

        public async Task<TOut> HandleAsync(TRequest request, CancellationToken token)
        {
            var entity = await SaveEntity(request, token).Configure();
            var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, tOut, token).Configure();
                
            return result;
        }
    }
}
