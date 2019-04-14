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
    internal abstract class DeleteRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : IDeleteRequest
    {
        protected DeleteRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }
        
        protected async Task<(TEntity, bool)> DeleteEntity(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();

            ct.ThrowIfCancellationRequested();

            var found = entity != null;
            
            if (found)
            {
                entity = await Context.Set<TEntity>().DeleteAsync(entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

                await Context.ApplyChangesAsync(ct).Configure();
            }

            ct.ThrowIfCancellationRequested();

            return (entity, found);
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IDeleteRequest<TEntity>
    {
        public DeleteRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    (_, var found) = await DeleteEntity(request, ct).Configure();
                    if (!found && RequestConfig.ErrorConfig.FailedToFindInDeleteIsError)
                        return ErrorDispatcher.Dispatch(new FailedToFindError(request, typeof(TEntity)));
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(RequestFailedError.From(request, e));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(RequestCanceledError.From(request, e));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(HookFailedError.From(request, e));
                }
            }

            return Response.Success();
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity, TOut>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IDeleteRequest<TEntity, TOut>
    {
        public DeleteRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var result = default(TOut);

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    var (entity, found) = await DeleteEntity(request, ct).Configure();
                    if (!found && RequestConfig.ErrorConfig.FailedToFindInDeleteIsError)
                        return ErrorDispatcher.Dispatch<TOut>(new FailedToFindError(request, typeof(TEntity)));

                    var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig, ct).Configure();
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
    }
}
