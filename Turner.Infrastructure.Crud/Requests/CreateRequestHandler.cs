using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class CreateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected CreateRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity> CreateEntity(TRequest request, CancellationToken ct)
        {
            var requestHooks = RequestConfig.GetRequestHooks(request);
            foreach (var hook in requestHooks)
                await hook.Run(request, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var data = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);
            var creator = RequestConfig.GetCreatorFor<TEntity>();
            var entity = await creator(request, data, ct).Configure();

            ct.ThrowIfCancellationRequested();

            entity = await Context.EntitySet<TEntity>().CreateAsync(entity, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>(request);
            foreach (var hook in entityHooks)
            {
                await hook.Run(request, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }

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

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
                await CreateEntity(request, cts.Token).Configure();

            return Response.Success();
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

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var resultHooks = RequestConfig.GetResultHooks(request);
                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();

                var entity = await CreateEntity(request, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var result = await transform(entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                foreach (var hook in resultHooks)
                    result = (TOut)await hook.Run(request, result, ct).Configure();

                ct.ThrowIfCancellationRequested();
                
                return result.AsResponse();
            }
        }
    }
}
