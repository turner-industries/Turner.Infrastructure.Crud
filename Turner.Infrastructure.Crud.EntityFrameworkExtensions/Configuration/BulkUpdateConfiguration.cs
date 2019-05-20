using System;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration;
using Z.BulkOperations;

namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration
{
    public class BulkUpdateConfiguration<TEntity> : BulkConfiguration<TEntity, BulkUpdateConfiguration<TEntity>>
        where TEntity : class
    {
        private bool _allowDuplicateKeys;
        private CaseSensitiveType _keyCaseSensitivity = CaseSensitiveType.Sensitive;
        
        public override BulkOperation<TOperationEntity> Apply<TOperationEntity>(
            ICrudRequestConfig config, 
            BulkOperation<TOperationEntity> operation)
        {
            operation = base.Apply(config, operation);

            operation.AllowDuplicateKeys = _allowDuplicateKeys;
            operation.CaseSensitive = _keyCaseSensitivity;

            if (IgnoredColumns.Count > 0)
            {
                if (operation.IgnoreOnUpdateExpression != null)
                {
                    foreach (var member in ((NewExpression)operation.IgnoreOnUpdateExpression.Body).Members)
                        IgnoredColumns.Add(member);
                }

                operation.IgnoreOnUpdateExpression = CreateNewExpression<TOperationEntity>(IgnoredColumns);
            }

            return operation;
        }
        
        public BulkUpdateConfiguration<TEntity> AllowDuplicateKeys(bool shouldAllow)
        {
            _allowDuplicateKeys = shouldAllow;

            return this;
        }

        public BulkUpdateConfiguration<TEntity> WithKeyCaseSensitivity(CaseSensitiveType sensitivityType)
        {
            _keyCaseSensitivity = sensitivityType;

            return this;
        }
        
        public BulkUpdateConfiguration<TEntity> IgnoreColumns(params Expression<Func<TEntity, object>>[] members)
        {
            foreach (var memberExpr in members)
                foreach (var member in GetMembers(memberExpr))
                    IgnoredColumns.Add(member);

            return this;
        }
    }
}
