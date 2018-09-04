using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IGetAlgorithm
    {
        DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class;
    }

    public class StandardGetAlgorithm : IGetAlgorithm
    {
        private readonly IContextAccess _contextAccess;

        public StandardGetAlgorithm(IContextAccess contextAccess)
        {
            _contextAccess = contextAccess;
        }

        public DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class
        {
            return _contextAccess.GetEntities<TEntity>(context);
        }
    }

    internal class GetRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IGetRequest<TEntity, TOut>
    {
        protected readonly IGetAlgorithm Algorithm;

        public GetRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            IGetAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>(SelectorType.Get);
            var entity = await Algorithm.GetEntities<TEntity>(Context)
                .SelectAsync(request, selector)
                .Configure();

            var failedToFind = entity == null;
            
            if (failedToFind)
                entity = RequestConfig.GetDefault<TEntity>();
                
            var result = Mapper.Map<TOut>(entity);

            if (failedToFind && RequestConfig.ErrorConfig.FailedToFindInGetIsError)
            {
                var error = new FailedToFindException("Failed to find entity.")
                {
                    RequestTypeProperty = request.GetType(),
                    QueryTypeProperty = typeof(TEntity),
                    ResponseData = result
                };

                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            return new Response<TOut> { Data = result };
        }
    }
}
