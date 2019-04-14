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
        
        protected async Task<TEntity> UpdateEntity(TRequest request, CancellationToken ct)
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

                entity = await Context.Set<TEntity>().UpdateAsync(entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

                await Context.ApplyChangesAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            else if (RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
            {
                throw new CrudFailedToFindException { EntityTypeProperty = typeof(TEntity) };
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
            return HandleWithErrorsAsync(request, (_, token) => (Task)UpdateEntity(request, token));
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
            return HandleWithErrorsAsync(request, HandleAsync);
        }

        public async Task<TOut> HandleAsync(TRequest request, CancellationToken token)
        {
            var entity = await UpdateEntity(request, token).Configure();
            var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, tOut, token).Configure();

            return result;
        }
    }
}
