using Turner.Infrastructure.Crud.Configuration;
using Z.BulkOperations;

namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration
{
    public class BulkDeleteConfiguration<TEntity> 
        : BulkConfiguration<TEntity, BulkDeleteConfiguration<TEntity>>
        where TEntity : class
    {
        private CaseSensitiveType _keyCaseSensitivity = CaseSensitiveType.Sensitive;

        public override BulkOperation<TOperationEntity> Apply<TOperationEntity>(
            IRequestConfig config, 
            BulkOperation<TOperationEntity> operation)
        {
            operation = base.Apply(config, operation);
            
            operation.CaseSensitive = _keyCaseSensitivity;

            return operation;
        }

        public BulkDeleteConfiguration<TEntity> WithKeyCaseSensitivity(CaseSensitiveType sensitivityType)
        {
            _keyCaseSensitivity = sensitivityType;

            return this;
        }
    }
}
