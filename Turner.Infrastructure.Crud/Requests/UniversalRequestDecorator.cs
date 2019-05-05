using System;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public class UniversalRequestDecorator<TRequest>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly ICrudRequestConfig _requestConfig;
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;

        public UniversalRequestDecorator(CrudConfigManager profileManager,
            Func<IRequestHandler<TRequest>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                foreach (var requestHook in _requestConfig.GetRequestHooks())
                {
                    await requestHook.Run(request, token).Configure();
                    token.ThrowIfCancellationRequested();
                }

                return await _decorateeFactory().HandleAsync(request);
            }
        }
    }

    public class UniversalRequestDecorator<TRequest, TResult>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        private readonly ICrudRequestConfig _requestConfig;
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;

        public UniversalRequestDecorator(CrudConfigManager profileManager,
            Func<IRequestHandler<TRequest, TResult>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                foreach (var requestHook in _requestConfig.GetRequestHooks())
                {
                    await requestHook.Run(request, token).Configure();
                    token.ThrowIfCancellationRequested();
                }

                var response = await _decorateeFactory().HandleAsync(request);

                if (response.HasErrors)
                    return response;

                var result = response.Data;

                foreach (var hook in _requestConfig.GetResultHooks())
                {
                    if (typeof(TResult).IsAssignableFrom(hook.ResultType))
                        result = (TResult)await hook.Run(request, result, token).Configure();
                    else
                        result = await ResultHookAdapter.Adapt(hook, request, result, token).Configure();

                    token.ThrowIfCancellationRequested();
                }

                return result.AsResponse();
            }
        }
    }
}
