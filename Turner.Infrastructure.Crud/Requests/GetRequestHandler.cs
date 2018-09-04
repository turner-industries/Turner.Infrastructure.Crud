using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
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
        protected readonly RequestOptions Options;

        public GetRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            IGetAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var entity = default(TEntity);
            var result = default(TOut);
            bool failedToFind = false;

            try
            {
                var selector = RequestConfig.GetSelectorFor<TEntity>(SelectorType.Get);

                if (Options.UseProjection)
                {
                    result = await Algorithm.GetEntities<TEntity>(Context)
                        .ProjectSingleAsync<TRequest, TEntity, TOut>(request, selector)
                        .Configure();

                    if (result == null)
                    {
                        failedToFind = true;
                        result = Mapper.Map<TOut>(RequestConfig.GetDefault<TEntity>());
                    }
                }
                else
                {
                    entity = await Algorithm.GetEntities<TEntity>(Context)
                        .SelectSingleAsync(request, selector)
                        .Configure();

                    if (entity == null)
                    {
                        failedToFind = true;
                        entity = RequestConfig.GetDefault<TEntity>();
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
