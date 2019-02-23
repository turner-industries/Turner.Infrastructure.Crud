using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class SaveRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly RequestOptions Options;

        protected SaveRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected Task<TEntity> GetEntity(TRequest request, CancellationToken ct)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var set = Context.EntitySet<TEntity>();
            
            return Context.SingleOrDefaultAsync(set, selector(request), ct);
        }

        protected async Task<TEntity> SaveEntity(TRequest request, TEntity entity, CancellationToken ct)
        {
            var requestHooks = RequestConfig.GetRequestHooks();
            foreach (var hook in requestHooks)
                await hook.Run(request, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            if (entity == null)
            {
                entity = await CreateEntity(request, item, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>();
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity, ct).Configure();

                ct.ThrowIfCancellationRequested();

                await Context.ApplyChangesAsync(ct).Configure();
            }
            else
            {
                entity = await UpdateEntity(request, item, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>();
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity, ct).Configure();

                ct.ThrowIfCancellationRequested();

                await Context.ApplyChangesAsync(ct).Configure();
            }

            ct.ThrowIfCancellationRequested();

            return entity;
        }

        private async Task<TEntity> CreateEntity(TRequest request, object data, CancellationToken ct)
        {
            var creator = RequestConfig.GetCreatorFor<TEntity>();
            var entity = await creator(request, data, ct).Configure();

            ct.ThrowIfCancellationRequested();

            entity = await Context.EntitySet<TEntity>().CreateAsync(entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }

        private async Task<TEntity> UpdateEntity(TRequest request, object data, TEntity entity, CancellationToken ct)
        {
            var updator = RequestConfig.GetUpdatorFor<TEntity>();
            await updator(request, data, entity, ct).Configure();

            ct.ThrowIfCancellationRequested();

            entity = await Context.EntitySet<TEntity>().UpdateAsync(entity, ct).Configure();
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

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                TEntity entity;

                try
                {
                    entity = await GetEntity(request, ct).Configure();
                }
                catch (CrudRequestFailedException e)
                {
                    var error = new RequestFailedError(request, e);
                    return ErrorDispatcher.Dispatch(error);
                }

                ct.ThrowIfCancellationRequested();

                await SaveEntity(request, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }

            return Response.Success();
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

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            TOut result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                TEntity entity;

                try
                {
                    entity = await GetEntity(request, ct).Configure();
                }
                catch (CrudRequestFailedException e)
                {
                    var error = new RequestFailedError(request, e);
                    return ErrorDispatcher.Dispatch<TOut>(error);
                }

                ct.ThrowIfCancellationRequested();

                var newEntity = await SaveEntity(request, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                result = await transform(newEntity, ct).Configure();

                ct.ThrowIfCancellationRequested();

                var resultHooks = RequestConfig.GetResultHooks();
                foreach (var hook in resultHooks)
                    result = (TOut)await hook.Run(request, result, ct).Configure();

                ct.ThrowIfCancellationRequested();
            }

            return result.AsResponse();
        }
    }
}
