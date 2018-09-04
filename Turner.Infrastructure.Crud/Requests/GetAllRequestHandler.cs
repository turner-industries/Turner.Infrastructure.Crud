using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IGetAllAlgorithm
    {
        DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class;
    }

    public class StandardGetAllAlgorithm : IGetAllAlgorithm
    {
        private readonly IContextAccess _contextAccess;

        public StandardGetAllAlgorithm(IContextAccess contextAccess)
        {
            _contextAccess = contextAccess;
        }

        public DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class
        {
            return _contextAccess.GetEntities<TEntity>(context);
        }
    }

    internal class GetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, List<TOut>>
        where TEntity : class
        where TRequest : IGetAllRequest<TEntity, TOut>
    {
        protected readonly IGetAllAlgorithm Algorithm;
        protected readonly RequestOptions Options;

        public GetAllRequestHandler(DbContext context,
            CrudConfigManager profileManager,
            IGetAllAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<List<TOut>>> HandleAsync(TRequest request)
        {
            var result = default(List<TOut>);
            
            if (Options.UseProjection)
            {
                result = await Algorithm.GetEntities<TEntity>(Context)
                    .ProjectTo<TOut>()
                    .ToListAsync()
                    .Configure();
            }
            else
            {
                var entities = await Algorithm.GetEntities<TEntity>(Context)
                    .ToListAsync()
                    .Configure();

                result = Mapper.Map<List<TOut>>(entities);
            }

            if (result.Count == 0)
            {
                var defaultValue = RequestConfig.GetDefault<TEntity>();
                if (defaultValue != null)
                    result.Add(Mapper.Map<TOut>(defaultValue));

                if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                {
                    var error = new FailedToFindError(request, typeof(TEntity), result);
                    return ErrorDispatcher.Dispatch<List<TOut>>(error);
                }
            }

            return result.AsResponse();
        }
    }
}
