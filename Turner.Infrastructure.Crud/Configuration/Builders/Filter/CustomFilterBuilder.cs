using System;
using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Filter
{
    public class CustomFilterBuilder<TRequest, TEntity>
        : FilterBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> _filterFunc;

        public CustomFilterBuilder(Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> filterFunc)
        {
            _filterFunc = filterFunc;
        }

        internal override IFilter Build()
        {
            var filter = new CustomFilter();
            filter.SetFilter(_filterFunc);

            return filter;
        }
    }
}
