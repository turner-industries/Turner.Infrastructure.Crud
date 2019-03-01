using System;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Sort
{
    public class SortOperationBuilder<TEntity, TProp>
        where TEntity : class
    {
        public Expression<Func<TEntity, TProp>> Clause { get; set; }

        public SortDirection Direction { get; set; } = SortDirection.Default;
    }
}
