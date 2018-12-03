using System;
using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Filter
{
    public class BasicFilterBuilder<TRequest, TEntity>
        : FilterBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> _filterFunc;
        private Func<TRequest, bool> _predicateFunc;

        public BasicFilterBuilder(Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> filterFunc)
        {
            _filterFunc = filterFunc;
        }

        public BasicFilterBuilder<TRequest, TEntity> When(Func<TRequest, bool> condition)
        {
            _predicateFunc = condition;

            return this;
        }

        internal override IFilter Build()
        {
            var filter = new BasicFilter();
            filter.SetFilter(_filterFunc, _predicateFunc);

            return filter;
        }
    }
}
