using AutoMapper;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
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
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Create, request).Configure();

            var entity = await RequestConfig.CreateEntity<TEntity>(request).Configure();
            entity = await Context.EntitySet<TEntity>().CreateAsync(entity).Configure();

            await RequestConfig.RunPostActionsFor(ActionType.Create, request, entity).Configure();
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
