using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class UpdateAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly RequestOptions Options;

        protected UpdateAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }
        
        protected async Task<TEntity[]> UpdateEntities(TRequest request)
        {
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Update, request).Configure();

            var data = RequestConfig.GetRequestDataFor<TEntity>();

            var entities = await GetEntities(request).Configure();
            var items = (IEnumerable<object>)data.DataSource(request);
            var updator = RequestConfig.GetUpdatorFor<TEntity>();

            var updatedEntities = new List<TEntity>(entities.Length);
            foreach (var item in RequestConfig.Join(items, entities))
            {
                if (item.Item1 == null || item.Item2 == null)
                    continue;

                updatedEntities.Add(await updator(item.Item1, item.Item2).Configure());
            }

            entities = await Context.EntitySet<TEntity>().UpdateAsync(updatedEntities).Configure();

            foreach (var entity in entities)
                await RequestConfig.RunPostActionsFor(ActionType.Update, request, entity).Configure();

            await Context.ApplyChangesAsync().Configure();

            return entities;
        }

        private async Task<TEntity[]> GetEntities(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
            var entities = Context.EntitySet<TEntity>().AsQueryable();
            
            entities = entities.Where(selector(request));
            entities = RequestConfig
                .GetFiltersFor<TEntity>()
                .Aggregate(entities, (current, filter) => filter.Filter(request, current));

            return await Context.ToArrayAsync(entities).Configure();
        }
    }

    internal class UpdateAllRequestHandler<TRequest, TEntity>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IUpdateAllRequest<TEntity>
    {
        public UpdateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            await UpdateEntities(request).Configure();

            return Response.Success();
        }
    }

    internal class UpdateAllRequestHandler<TRequest, TEntity, TOut>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, UpdateAllResult<TOut>>
        where TEntity : class
        where TRequest : IUpdateAllRequest<TEntity, TOut>
    {
        public UpdateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<UpdateAllResult<TOut>>> HandleAsync(TRequest request)
        {
            var entities = await UpdateEntities(request).Configure();
            var result = new UpdateAllResult<TOut>(Mapper.Map<List<TOut>>(entities));

            return result.AsResponse();
        }
    }
}
