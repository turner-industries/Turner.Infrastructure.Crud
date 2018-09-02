using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class GetRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest>, IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IGetRequest<TEntity, TOut>
    {
        public GetRequestHandler(DbContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var selector = RequestConfig.GetSelector<TEntity>();
            var entity = await Context.Set<TEntity>()
                .SelectAsync(request, selector);

            var failedToFind = entity == null;
            
            if (failedToFind)
                entity = RequestConfig.GetDefault<TEntity>();
                
            var result = Mapper.Map<TOut>(entity);

            if (failedToFind && RequestConfig.FailedToFindInGetIsError)
            {
                throw new FailedToFindException("Failed to find entity.")
                {
                    RequestTypeProperty = request.GetType(),
                    QueryTypeProperty = typeof(TEntity),
                    ResponseData = result
                };
            }

            return new Response<TOut> { Data = result };
        }
    }
}
