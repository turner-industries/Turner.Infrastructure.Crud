using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class UpdateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : IUpdateRequest
    {
        protected UpdateRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }
        
        protected async Task<TEntity> UpdateEntity(TRequest request, 
            CancellationToken ct = default(CancellationToken))
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();
            
            ct.ThrowIfCancellationRequested();

            if (entity != null)
            {
                entity = await request.UpdateEntity(RequestConfig, item, entity, ct).Configure();

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

                entity = await Context.Set<TEntity>().UpdateAsync(DataContext, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                await Context.ApplyChangesAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            else if (RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
            {
                throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };
            }

            return entity;
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity>
    {
        public UpdateRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, _ => (Task)UpdateEntity(request));
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity, TOut>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity, TOut>
    {
        public UpdateRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response<TOut>> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, _HandleAsync);
        }

        public async Task<TOut> _HandleAsync(TRequest request)
        {
            var entity = await UpdateEntity(request).Configure();
            var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig).Configure();
            var result = await request.RunResultHooks(RequestConfig, tOut).Configure();

            return result;
        }
    }
}
