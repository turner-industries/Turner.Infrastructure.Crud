using AutoMapper;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Configuration;
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

        protected async Task<TEntity> CreateEntity(TRequest request)
        {
            var requestHooks = RequestConfig.GetRequestHooks(request);
            foreach (var hook in requestHooks)
                await hook.Run(request).Configure();

            var data = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);
            var creator = RequestConfig.GetCreatorFor<TEntity>();
            var entity = await creator(data).Configure();

            entity = await Context.EntitySet<TEntity>().CreateAsync(entity).Configure();

            var entityHooks = RequestConfig.GetEntityHooksFor<TEntity>(request);
            foreach (var hook in entityHooks)
                await hook.Run(request, entity).Configure();

            await Context.ApplyChangesAsync().Configure();

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
            await CreateEntity(request).Configure();

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
            var entity = await CreateEntity(request).Configure();
            var result = Mapper.Map<TOut>(entity);

            return result.AsResponse();
        }
    }
}
