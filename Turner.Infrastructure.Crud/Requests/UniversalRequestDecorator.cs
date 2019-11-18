using System;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public class UniversalRequestDecorator<TRequest>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly IRequestConfig _requestConfig;
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;

        public UniversalRequestDecorator(CrudConfigManager profileManager,
            Func<IRequestHandler<TRequest>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            foreach (var requestHook in _requestConfig.GetRequestHooks())
            {
                await requestHook.Run(request).Configure();
            }

            return await _decorateeFactory().HandleAsync(request);
        }
    }

    public class UniversalRequestDecorator<TRequest, TResult>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        private readonly IRequestConfig _requestConfig;
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;

        public UniversalRequestDecorator(CrudConfigManager profileManager,
            Func<IRequestHandler<TRequest, TResult>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request)
        {
            foreach (var requestHook in _requestConfig.GetRequestHooks())
            {
                await requestHook.Run(request).Configure();
            }

            var response = await _decorateeFactory().HandleAsync(request);

            if (response.HasErrors)
                return response;

            var result = response.Data;

            foreach (var hook in _requestConfig.GetResultHooks())
            {
                if (typeof(TResult).IsAssignableFrom(hook.ResultType))
                    result = (TResult)await hook.Run(request, result).Configure();
                else
                    result = await ResultHookAdapter.Adapt(hook, request, result).Configure();
            }

            return result.AsResponse();
        }
    }
}
