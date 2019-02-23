using AutoMapper.QueryableExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
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
            GetAllResult<TOut> result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                List<TOut> items;

                var requestHooks = RequestConfig.GetRequestHooks();
                foreach (var hook in requestHooks)
                    await hook.Run(request, ct).Configure();

                ct.ThrowIfCancellationRequested();

                var entities = Context.EntitySet<TEntity>().AsQueryable();

                foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                    entities = filter.Filter(request, entities).Cast<TEntity>();

                var sorter = RequestConfig.GetSorterFor<TEntity>();
                entities = sorter?.Sort(request, entities).Cast<TEntity>() ?? entities;

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();

                if (Options.UseProjection)
                {
                    items = await Context.ToListAsync(entities.ProjectTo<TOut>(), ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    var resultEntities = await Context.ToListAsync(entities, ct).Configure();
                    ct.ThrowIfCancellationRequested();

                    items = new List<TOut>(await Task.WhenAll(resultEntities.Select(x => transform(x, ct))).Configure());
                    ct.ThrowIfCancellationRequested();
                }

                if (items.Count == 0)
                {
                    var defaultValue = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultValue != null)
                    {
                        items.Add(await transform(defaultValue, ct).Configure());
                        ct.ThrowIfCancellationRequested();
                    }

                    if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                    {
                        var errorResult = new GetAllResult<TOut>(items);
                        var error = new FailedToFindError(request, typeof(TEntity), errorResult);

                        return ErrorDispatcher.Dispatch<GetAllResult<TOut>>(error);
                    }
                }

                var resultHooks = RequestConfig.GetResultHooks();
                foreach (var hook in resultHooks)
                    for (var i = 0; i < items.Count; ++i)
                        items[i] = (TOut)await hook.Run(request, items[i], ct).Configure();

                ct.ThrowIfCancellationRequested();

                result = new GetAllResult<TOut>(items);
            }

            return result.AsResponse();
        }
    }
}
