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
    
    public class MaybeValidateDecorator<TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest, ICrudRequest
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly ValidatorFactory _validatorFactory;

        public MaybeValidateDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            ValidatorFactory validatorFactory)
        {
            _decorateeFactory = decorateeFactory;
            _validatorFactory = validatorFactory;
        }

        public async Task<Response> HandleAsync(TRequest request)
        { 
            var handler = _decorateeFactory();

            var validator = _validatorFactory.TryCreate<TRequest>();
            if (validator == null)
                return await handler.HandleAsync(request);

            var errors = await validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await handler.HandleAsync(request);

            return new Response
            {
                Errors = Mapper.Map<List<Error>>(errors)
            };
        }
    }

    public class MaybeValidateDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>, ICrudRequest
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly ValidatorFactory _validatorFactory;

        public MaybeValidateDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            ValidatorFactory validatorFactory)
        {
            _decorateeFactory = decorateeFactory;
            _validatorFactory = validatorFactory;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request)
        {
            var handler = _decorateeFactory();

            var validator = _validatorFactory.TryCreate<TRequest>();
            if (validator == null)
                return await handler.HandleAsync(request);

            var errors = await validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await handler.HandleAsync(request);

            return new Response<TResult>
            {
                Errors = Mapper.Map<List<Error>>(errors)
            };
        }
    }
}
