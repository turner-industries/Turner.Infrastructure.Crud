using System;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Validation
{
    public class ValidateAttribute : Attribute
    {
        public Type ValidatorType { get; }

        public ValidateAttribute(Type validatorType)
        {
            if (validatorType != null && (
                validatorType.IsGenericTypeDefinition ||
                !validatorType.GetInterfaces().Any(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IValidator<>))))
            {
                string message = $"The type '{validatorType}' cannot be used for this attribute because " +
                                 $"it is not a concrete type implementing the IValidator interface.";

                throw new BadCrudConfigurationException(message);
            }

            ValidatorType = validatorType;
        }

        public ValidateAttribute() : this(null)
        {
        }
    }
    
    public class CrudValidationBaseDecorator<TRequest, TResponse, TValidator> 
        where TResponse : Response, new()
        where TValidator : IValidator<TRequest>
    {
        private readonly TValidator _validator;

        public CrudValidationBaseDecorator(TValidator validator)
        {
            _validator = validator;
        }

        public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> processRequest)
        {
            var errors = await _validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await processRequest();

            var response = new TResponse
            {
                Errors = errors
                    .Select(x => new Error
                    {
                        PropertyName = x.PropertyName,
                        ErrorMessage = x.ErrorMessage
                    })
                    .ToList()
            };

            return response;
        }
    }

    public class CrudValidationDecorator<TRequest, TValidator> 
        : IRequestHandler<TRequest> 
        where TRequest : IRequest, ICrudRequest
        where TValidator : IValidator<TRequest>
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly CrudValidationBaseDecorator<TRequest, Response, TValidator> _validationHandler;

        public CrudValidationDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            CrudValidationBaseDecorator<TRequest, Response, TValidator> validationHandler)
        {
            _decorateeFactory = decorateeFactory;
            _validationHandler = validationHandler;
        }

        public Task<Response> HandleAsync(TRequest request)
            => _validationHandler.HandleAsync(request, () => _decorateeFactory().HandleAsync(request));
    }

    public class CrudValidationDecorator<TRequest, TResult, TValidator> 
        : IRequestHandler<TRequest, TResult> 
        where TRequest : IRequest<TResult>, ICrudRequest
        where TValidator : IValidator<TRequest>
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly CrudValidationBaseDecorator<TRequest, Response<TResult>, TValidator> _validationHandler;

        public CrudValidationDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            CrudValidationBaseDecorator<TRequest, Response<TResult>, TValidator> validationHandler)
        {
            _decorateeFactory = decorateeFactory;
            _validationHandler = validationHandler;
        }

        public Task<Response<TResult>> HandleAsync(TRequest request)
            => _validationHandler.HandleAsync(request, () => _decorateeFactory().HandleAsync(request));
    }
}
