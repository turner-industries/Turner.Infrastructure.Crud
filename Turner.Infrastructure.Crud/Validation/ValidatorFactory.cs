using System;

namespace Turner.Infrastructure.Crud.Validation
{
    internal class ValidatorFactory
    {
        private readonly Func<Type, object> _validatorCreator;

        public ValidatorFactory(Func<Type, object> creator)
        {
            _validatorCreator = creator;
        }

        public IRequestValidator<TRequest> TryCreate<TRequest>()
        {
            try
            {
                return (IRequestValidator<TRequest>)_validatorCreator(typeof(IRequestValidator<TRequest>));
            }
            catch
            {
                return null;
            }
        }
    }
}
