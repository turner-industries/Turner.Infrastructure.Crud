using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
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

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var failedToFind = false;
                
                try
                {
                    var requestHooks = RequestConfig.GetRequestHooks();
                    foreach (var hook in requestHooks)
                        await hook.Run(request, ct).Configure();
                    ct.ThrowIfCancellationRequested();

                    var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
                    var entities = Context.Set<TEntity>().AsQueryable();
                    var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();

                    foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                        entities = filter.Filter(request, entities).Cast<TEntity>();

                    if (Options.UseProjection)
                    {
                        result = await entities
                            .Where(selector(request))
                            .ProjectTo<TOut>()
                            .SingleOrDefaultAsync(ct)
                            .Configure();

                        ct.ThrowIfCancellationRequested();

                        if (result == null)
                        {
                            failedToFind = true;
                            result = await transform(RequestConfig.GetDefaultFor<TEntity>(), ct).Configure();
                            ct.ThrowIfCancellationRequested();
                        }
                    }
                    else
                    {
                        var entity = await entities
                            .SingleOrDefaultAsync(selector(request), ct)
                            .Configure();

                        ct.ThrowIfCancellationRequested();

                        if (entity == null)
                        {
                            failedToFind = true;
                            entity = RequestConfig.GetDefaultFor<TEntity>();
                        }

                        result = await transform(entity, ct).Configure();
                        ct.ThrowIfCancellationRequested();
                    }
                }
                catch (CrudRequestFailedException e)
                {
                    var error = new RequestFailedError(request, e);
                    return ErrorDispatcher.Dispatch<TOut>(error);
                }

                if (failedToFind && RequestConfig.ErrorConfig.FailedToFindInGetIsError)
                {
                    var error = new FailedToFindError(request, typeof(TEntity), result);
                    return ErrorDispatcher.Dispatch<TOut>(error);
                }

                var resultHooks = RequestConfig.GetResultHooks();
                foreach (var hook in resultHooks)
                    result = (TOut)await hook.Run(request, result, ct).Configure();

                ct.ThrowIfCancellationRequested();
            }

            return result.AsResponse();
        }
    }
}
