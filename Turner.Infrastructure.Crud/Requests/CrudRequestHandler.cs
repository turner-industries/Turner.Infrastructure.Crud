using System;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class CrudRequestHandler<TRequest, TEntity> : ICrudRequestHandler
        where TEntity : class
    {
        protected readonly IEntityContext Context;
        protected readonly ICrudRequestConfig RequestConfig;
        protected readonly DataContext<TEntity> DataContext;
        
        protected CrudRequestHandler(IEntityContext context, CrudConfigManager profileManager)
        {
            Context = context;
            RequestConfig = profileManager.GetRequestConfigFor<TRequest>();
            DataContext = new DataContext<TEntity>(RequestConfig);

            var errorHandler = RequestConfig.ErrorConfig.GetErrorHandlerFor<TEntity>();
            ErrorDispatcher = new CrudErrorDispatcher(errorHandler);
        }

        protected async Task<Response> HandleWithErrorsAsync(TRequest request, Func<TRequest, CancellationToken, Task> handleAsync)
        {
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    await handleAsync(request, cts.Token).Configure();
                }
                catch (Exception e) when (FailedToFindError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(FailedToFindError.From(request, e));
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
                catch (Exception e) when (CreateEntityFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(CreateEntityFailedError.From(request, e));
                }
                catch (Exception e) when (UpdateEntityFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(UpdateEntityFailedError.From(request, e));
                }
                catch (Exception e) when (CreateResultFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(CreateResultFailedError.From(request, e));
                }
            }

            return Response.Success();
        }

        protected async Task<Response<TResult>> HandleWithErrorsAsync<TResult>(TRequest request, Func<TRequest, CancellationToken, Task<TResult>> handleAsync)
        {
            var result = default(TResult);

            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    result = await handleAsync(request, cts.Token).Configure();
                }
                catch (Exception e) when (FailedToFindError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TResult>(FailedToFindError.From(request, e));
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TResult>(RequestFailedError.From(request, e));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TResult>(RequestCanceledError.From(request, e));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TResult>(HookFailedError.From(request, e));
                }
                catch (Exception e) when (CreateEntityFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TResult>(CreateEntityFailedError.From(request, e));
                }
                catch (Exception e) when (UpdateEntityFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TResult>(UpdateEntityFailedError.From(request, e));
                }
                catch (Exception e) when (CreateResultFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<TResult>(CreateResultFailedError.From(request, e));
                }
            }

            return result.AsResponse();
        }

        public CrudErrorDispatcher ErrorDispatcher { get; }
    }
}
