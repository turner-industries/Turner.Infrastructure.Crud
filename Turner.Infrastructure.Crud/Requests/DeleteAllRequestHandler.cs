using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class DeleteAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly RequestOptions Options;

        protected DeleteAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> DeleteEntities(TRequest request)
        {
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Delete, request).Configure();

            var entities = await GetEntities(request);
            entities = await Context.EntitySet<TEntity>().DeleteAsync(entities).Configure();

            foreach (var entity in entities)
                await RequestConfig.RunPostActionsFor(ActionType.Delete, request, entity).Configure();

            await Context.ApplyChangesAsync().Configure();

            return entities;
        }
        
        private async Task<TEntity[]> GetEntities(TRequest request)
        {
            var entities = Context.EntitySet<TEntity>().AsQueryable();

            foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                entities = filter.Filter(request, entities);

            return await Context.ToArrayAsync(entities).Configure();
        }
    }

    internal class DeleteAllRequestHandler<TRequest, TEntity>
        : DeleteAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IDeleteAllRequest<TEntity>
    {
        public DeleteAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            await DeleteEntities(request).Configure();

            return Response.Success();
        }
    }

    internal class DeleteAllRequestHandler<TRequest, TEntity, TOut>
        : DeleteAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, DeleteAllResult<TOut>>
        where TEntity : class
        where TRequest : IDeleteAllRequest<TEntity, TOut>
    {
        public DeleteAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<DeleteAllResult<TOut>>> HandleAsync(TRequest request)
        {
            var entities = await DeleteEntities(request).Configure();
            var result = new DeleteAllResult<TOut>(Mapper.Map<List<TOut>>(entities));

            return result.AsResponse();
        }
    }
}
