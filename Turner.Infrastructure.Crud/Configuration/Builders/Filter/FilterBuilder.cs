using System;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Filter
{
    public abstract class FilterBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        internal abstract IFilter Build();
    }

    public class FilterBuilder<TRequest, TEntity>
        where TEntity : class
    {
        private FilterBuilderBase<TRequest, TEntity> _builder;
        
        public BasicFilterBuilder<TRequest, TEntity> FilterOn(
            Func<TRequest, Expression<Func<TEntity, int, bool>>> filterFunc)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) => queryable.Where(filterFunc(request)));

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn(
            Func<TRequest, Expression<Func<TEntity, bool>>> filterFunc)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>(
                (request, queryable) => queryable.Where(filterFunc(request)));

            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn(
            Expression<Func<TEntity, int, bool>> filterExpr)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>((request, queryable) => queryable.Where(filterExpr));
            _builder = builder;

            return builder;
        }

        public BasicFilterBuilder<TRequest, TEntity> FilterOn(
            Expression<Func<TEntity, bool>> filterExpr)
        {
            var builder = new BasicFilterBuilder<TRequest, TEntity>((request, queryable) => queryable.Where(filterExpr));
            _builder = builder;

            return builder;
        }

        public CustomFilterBuilder<TRequest, TEntity> Custom(
            Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> customFilterFunc)
        {
            var builder = new CustomFilterBuilder<TRequest, TEntity>(customFilterFunc);
            _builder = builder;

            return builder;
        }

        internal IFilter Build()
        {
            return _builder?.Build();
        }
    }
}
