using AutoMapper.QueryableExtensions;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
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
                var requestHooks = RequestConfig.GetRequestHooks(request);
                foreach (var hook in requestHooks)
                    await hook.Run(request).Configure();

                var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
                var entities = Context.EntitySet<TEntity>().AsQueryable();
                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();

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
                        result = await transform(RequestConfig.GetDefaultFor<TEntity>()).Configure();
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

                    result = await transform(entity).Configure();
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

            var resultHooks = RequestConfig.GetResultHooks(request);
            foreach (var hook in resultHooks)
                result = (TOut)await hook.Run(request, result).Configure();

            return result.AsResponse();
        }
    }
}
