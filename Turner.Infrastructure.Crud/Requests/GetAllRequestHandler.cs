using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class GetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, GetAllResult<TOut>>
        where TEntity : class
        where TRequest : IGetAllRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public GetAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<GetAllResult<TOut>>> HandleAsync(TRequest request)
        {
            List<TOut> items;

            var requestHooks = RequestConfig.GetRequestHooks(request);
            foreach (var hook in requestHooks)
                await hook.Run(request).Configure();

            var entities = Context.EntitySet<TEntity>().AsQueryable();

            foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                entities = filter.Filter(request, entities);

            var sorter = RequestConfig.GetSorterFor<TEntity>();
            entities = sorter?.Sort(request, entities) ?? entities;

            items = await entities.ProjectResults<TEntity, TOut>(Context, Options).Configure();

            if (items.Count == 0)
            {
                var defaultValue = RequestConfig.GetDefaultFor<TEntity>();
                if (defaultValue != null)
                    items.Add(Mapper.Map<TOut>(defaultValue));

                if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                {
                    var errorResult = new GetAllResult<TOut>(items);
                    var error = new FailedToFindError(request, typeof(TEntity), errorResult);

                    return ErrorDispatcher.Dispatch<GetAllResult<TOut>>(error);
                }
            }

            var result = new GetAllResult<TOut>(items);

            return result.AsResponse();
        }
    }
}
