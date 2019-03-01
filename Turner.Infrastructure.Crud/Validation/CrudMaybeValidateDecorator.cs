using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Validation
{
    internal class MaybeValidateAttribute : Attribute
    {
    }

    public class CrudMaybeValidateBaseDecorator<TRequest, TResponse>
        where TResponse : Response, new()
    {
        private readonly ValidatorFactory _validatorFactory;

        public CrudMaybeValidateBaseDecorator(ValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory;
        }

        public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> processRequest)
        {
            var validator = _validatorFactory.TryCreate<TRequest>();
            if (validator == null)
                return await processRequest();

            var errors = await validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await processRequest();
            
            return new TResponse { Errors = Mapper.Map<List<Error>>(errors) };
        }
    }

    public class CrudMaybeValidateDecorator<TRequest>
        : IRequestHandler<TRequest>
        where TRequest : IRequest, ICrudRequest
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly CrudMaybeValidateBaseDecorator<TRequest, Response> _validationHandler;

        public CrudMaybeValidateDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            CrudMaybeValidateBaseDecorator<TRequest, Response> validationHandler)
        {
            _decorateeFactory = decorateeFactory;
            _validationHandler = validationHandler;
        }

        public Task<Response> HandleAsync(TRequest request)
            => _validationHandler.HandleAsync(request, () => _decorateeFactory().HandleAsync(request));
    }

    public class CrudMaybeValidateDecorator<TRequest, TResult>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>, ICrudRequest
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly CrudMaybeValidateBaseDecorator<TRequest, Response<TResult>> _validationHandler;

        public CrudMaybeValidateDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            CrudMaybeValidateBaseDecorator<TRequest, Response<TResult>> validationHandler)
        {
            _decorateeFactory = decorateeFactory;
            _validationHandler = validationHandler;
        }

        public Task<Response<TResult>> HandleAsync(TRequest request)
            => _validationHandler.HandleAsync(request, () => _decorateeFactory().HandleAsync(request));
    }
}
