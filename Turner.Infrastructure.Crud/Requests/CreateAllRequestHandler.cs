using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class CreateAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected CreateAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity[]> CreateEntities(TRequest request)
        {
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Create, request).Configure();
            
            var entities = await RequestConfig.CreateEntities<TEntity>(request).Configure();
            entities = await Context.EntitySet<TEntity>().CreateAsync(entities).Configure();

            foreach (var entity in entities)
                await RequestConfig.RunPostActionsFor(ActionType.Create, request, entity).Configure();

            await Context.ApplyChangesAsync().Configure();

            return entities;
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity>
    {
        public CreateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            await CreateEntities(request).Configure();

            return Response.Success();
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity, TOut>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, CreateAllResult<TOut>>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity, TOut>
    {
        public CreateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<CreateAllResult<TOut>>> HandleAsync(TRequest request)
        {
            var entities = await CreateEntities(request).Configure();
            var result = new CreateAllResult<TOut>(Mapper.Map<List<TOut>>(entities));

            return result.AsResponse();
        }
    }
}
