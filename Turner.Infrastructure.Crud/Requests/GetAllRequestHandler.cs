using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
            List<TOut> result;

            var entities = Algorithm
                .GetEntities<TEntity>(Context)
                .AsQueryable();
            
            var sorters = RequestConfig.GetSortersFor<TEntity>(SorterType.GetAll);

            if (sorters != null)
            {
                IOrderedQueryable<TEntity> sortedEntities = null;

                foreach (var sorter in sorters)
                {
                    sortedEntities = sorter.Sort(request, entities);
                    if (sortedEntities != null)
                        break;
                }

                if (sortedEntities != null)
                    entities = sortedEntities;
            }

            if (Options.UseProjection)
            {
                result = await entities
                    .ProjectTo<TOut>()
                    .ToListAsync()
                    .Configure();
            }
            else
            {
                var resultEntities = await entities   
                    .ToListAsync()
                    .Configure();

                result = Mapper.Map<List<TOut>>(resultEntities);
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
