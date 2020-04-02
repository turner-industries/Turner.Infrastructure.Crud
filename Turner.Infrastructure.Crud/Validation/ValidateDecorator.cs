using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Validation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class DoNotValidateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ValidateAttribute : Attribute
    {
        public Type ValidatorType { get; }

        public ValidateAttribute(Type validatorType)
        {
            if (validatorType != null && (
                validatorType.IsGenericTypeDefinition ||
                !validatorType.GetInterfaces().Any(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestValidator<>))))
            {
                var message = $"The type '{validatorType}' cannot be used for this attribute because " +
                              "it is not a concrete type implementing the IRequestValidator interface.";

                throw new Exception(message);
            }

            ValidatorType = validatorType;
        }

        public ValidateAttribute() : this(null)
        {
        }
    }

    public class ValidateDecorator<TRequest, TValidator>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
        where TValidator : IRequestValidator<TRequest>
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly TValidator _validator;

        public ValidateDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            TValidator validator)
        {
            _decorateeFactory = decorateeFactory;
            _validator = validator;
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            var handler = _decorateeFactory();

            var errors = await _validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await handler.HandleAsync(request);

            return new Response
            {
                Errors = Mapper.Map<List<Error>>(errors)
            };
        }
    }

    public class ValidateDecorator<TRequest, TResult, TValidator>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
        where TValidator : IRequestValidator<TRequest>
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly TValidator _validator;

        public ValidateDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            TValidator validator)
        {
            _decorateeFactory = decorateeFactory;
            _validator = validator;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request)
        {
            var handler = _decorateeFactory();

            var errors = await _validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await handler.HandleAsync(request);

            return new Response<TResult>
            {
                Errors = Mapper.Map<List<Error>>(errors)
            };
        }
    }
}