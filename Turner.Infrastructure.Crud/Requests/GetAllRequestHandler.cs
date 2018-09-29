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
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, GetAllResult<TOut>>
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

        public async Task<Response<GetAllResult<TOut>>> HandleAsync(TRequest request)
        {
            List<TOut> items;

            var entities = Algorithm
                .GetEntities<TEntity>(Context)
                .AsQueryable();

            var sorter = RequestConfig.GetSorterFor<TEntity>(SorterType.GetAll);
            entities = sorter?.Sort(request, entities) ?? entities;

            if (Options.UseProjection)
            {
                items = await entities
                    .ProjectTo<TOut>()
                    .ToListAsync()
                    .Configure();
            }
            else
            {
                var resultEntities = await entities   
                    .ToListAsync()
                    .Configure();

                items = Mapper.Map<List<TOut>>(resultEntities);
            }

            if (items.Count == 0)
            {
                var defaultValue = RequestConfig.GetDefault<TEntity>();
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
