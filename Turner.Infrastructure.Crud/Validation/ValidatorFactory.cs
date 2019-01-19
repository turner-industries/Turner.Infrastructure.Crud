using System;

namespace Turner.Infrastructure.Crud.Validation
{
    public class ValidatorFactory
    {
        private readonly Func<Type, object> _validatorCreator;

        public ValidatorFactory(Func<Type, object> creator)
        {
            _validatorCreator = creator;
        }

        public IValidator<TRequest> TryCreate<TRequest>()
        {
            try
            {
                return (IValidator<TRequest>)_validatorCreator(typeof(IValidator<TRequest>));
            }
            catch
            {
                return null;
            }
        }
    }
}
