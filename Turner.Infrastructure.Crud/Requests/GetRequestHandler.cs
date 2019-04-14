using System;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
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
            var result = default(TOut);

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                { 
                    await request.RunRequestHooks(RequestConfig, ct).Configure();

                    var (tOut, found) = await FindEntity(request, ct).Configure();
                    if (!found && RequestConfig.ErrorConfig.FailedToFindInGetIsError)
                        return ErrorDispatcher.Dispatch<TOut>(new FailedToFindError(request, typeof(TEntity), result));
                    
                    result = await request.RunResultHooks(RequestConfig, tOut, ct).Configure();
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TOut>(RequestFailedError.From(request, e, result));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TOut>(RequestCanceledError.From(request, e, result));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TOut>(HookFailedError.From(request, e, result));
                }
            }

            return result.AsResponse();
        }

        private async Task<(TOut, bool)> FindEntity(TRequest request, CancellationToken ct)
        {
            var found = false;
            var result = default(TOut);
 
            var entities = Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SelectWith(request, RequestConfig);

            if (Options.UseProjection)
            {
                result = await entities.ProjectSingleOrDefaultAsync<TEntity, TOut>(ct).Configure();
                ct.ThrowIfCancellationRequested();
                found = result != null;
                
                if (!found)
                {
                    result = await RequestConfig.GetDefaultFor<TEntity>()
                        .CreateResult<TEntity, TOut>(RequestConfig, ct)
                        .Configure();
                }
            }
            else
            {
                var entity = await entities.SingleOrDefaultAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();
                found = entity != null;

                if (!found)
                    entity = RequestConfig.GetDefaultFor<TEntity>();

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

                result = await entity
                    .CreateResult<TEntity, TOut>(RequestConfig, ct)
                    .Configure();
            }

            ct.ThrowIfCancellationRequested();

            return (result, found);
        }
    }
}
