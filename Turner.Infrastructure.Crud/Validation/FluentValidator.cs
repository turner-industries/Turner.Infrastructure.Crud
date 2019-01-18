using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Validation
{
    public class FluentValidator<TRequest> : IValidator<TRequest>
    {
        private readonly FluentValidation.IValidator<TRequest> _validator;

        public FluentValidator(FluentValidation.IValidator<TRequest> validator)
        {
            _validator = validator;
        }

        public async Task<List<Error>> ValidateAsync(TRequest request, CancellationToken token = default(CancellationToken))
        {
            var validationResult = await _validator.ValidateAsync(request, token);
            if (validationResult.IsValid)
                return null;

            return validationResult.Errors
                .Select(error => new Error
                {
                    ErrorMessage = error.ErrorMessage,
                    PropertyName = error.PropertyName
                })
                .ToList();
        }
    }
}
