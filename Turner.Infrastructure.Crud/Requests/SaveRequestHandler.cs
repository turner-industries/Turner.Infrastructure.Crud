using AutoMapper;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
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

        protected Task<TEntity> GetEntity(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var set = Context.EntitySet<TEntity>();
            
            return Context.SingleOrDefaultAsync(set, selector(request));
        }

        protected async Task<TEntity> SaveEntity(TRequest request, TEntity entity)
        {
            var requestHooks = RequestConfig.GetRequestHooks(request);
            foreach (var hook in requestHooks)
                await hook.Run(request).Configure();

            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            if (entity == null)
            {
                entity = await CreateEntity(request, item).Configure();
                var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>(request);
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity).Configure();

                await Context.ApplyChangesAsync().Configure();
            }
            else
            {
                entity = await UpdateEntity(request, item, entity).Configure();
                var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>(request);
                foreach (var hook in entityHooks)
                    await hook.Run(request, entity).Configure();

                await Context.ApplyChangesAsync().Configure();
            }

            return entity;
        }

        private async Task<TEntity> CreateEntity(TRequest request, object data)
        {
            var creator = RequestConfig.GetCreatorFor<TEntity>();
            var entity = await creator(data).Configure();
            entity = await Context.EntitySet<TEntity>().CreateAsync(entity).Configure();

            return entity;
        }

        private async Task<TEntity> UpdateEntity(TRequest request, object data, TEntity entity)
        {
            var updator = RequestConfig.GetUpdatorFor<TEntity>();
            await updator(data, entity).Configure();
            entity = await Context.EntitySet<TEntity>().UpdateAsync(entity).Configure();

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
            TEntity entity;

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch(error);
            }

            await SaveEntity(request, entity).Configure();

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
            TEntity entity;

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            var newEntity = await SaveEntity(request, entity).Configure();
            var result = Mapper.Map<TOut>(newEntity);

            return result.AsResponse();
        }
    }
}
