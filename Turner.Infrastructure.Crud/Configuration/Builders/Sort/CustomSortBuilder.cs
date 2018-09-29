using System;
using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Sort
{
    public class CustomSortBuilder<TRequest, TEntity>
        : SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> _sortFunc;

        public CustomSortBuilder(Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
        {
            _sortFunc = sortFunc;
        }

        internal override ISorter Build()
        {
            var sorter = new CustomSorter();
            sorter.SetSort(_sortFunc);

            return sorter;
        }
    }
}
