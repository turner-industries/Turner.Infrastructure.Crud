using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class GetRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IGetRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public GetRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            TOut result;
            var failedToFind = false;

            try
            {
                var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
                var entities = Context.EntitySet<TEntity>().AsQueryable();

                foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                    entities = filter.Filter(request, entities);

                if (Options.UseProjection)
                {
                    result = await Context
                        .SingleOrDefaultAsync(entities.Where(selector(request)).ProjectTo<TOut>())
                        .Configure();
                    
                    if (result == null)
                    {
                        failedToFind = true;
                        result = Mapper.Map<TOut>(RequestConfig.GetDefaultFor<TEntity>());
                    }
                }
                else
                {
                    var entity = await Context
                        .SingleOrDefaultAsync(entities, selector(request))
                        .Configure();

                    if (entity == null)
                    {
                        failedToFind = true;
                        entity = RequestConfig.GetDefaultFor<TEntity>();
                    }

                    result = Mapper.Map<TOut>(entity);
                }
            }
            catch(CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            if (failedToFind && RequestConfig.ErrorConfig.FailedToFindInGetIsError)
            {
                var error = new FailedToFindError(request, typeof(TEntity), result);
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            return result.AsResponse();
        }
    }
}
