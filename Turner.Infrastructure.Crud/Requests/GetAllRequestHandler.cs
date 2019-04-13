using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
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

                await request.RunRequestHooks(RequestConfig.GetRequestHooks(), ct).Configure();

                ct.ThrowIfCancellationRequested();

                var entities = Context.Set<TEntity>()
                    .FilterWith(request, RequestConfig.GetFiltersFor<TEntity>())
                    .SortWith(request, RequestConfig.GetSorterFor<TEntity>());

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();

                if (Options.UseProjection)
                {
                    items = await entities.ProjectTo<TOut>().ToListAsync(ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    var resultEntities = await entities.ToListAsync(ct).Configure();
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

                items = await request.RunResultHooks(RequestConfig.GetResultHooks(), items, ct);

                result = new GetAllResult<TOut>(items);
            }

            return result.AsResponse();
        }
    }
}
