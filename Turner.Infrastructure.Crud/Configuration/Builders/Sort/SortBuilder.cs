using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Sort
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public abstract class SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        internal abstract ISorter Build();
    }

    public class SortBuilder<TRequest, TEntity>
        where TEntity : class
    {
        private SortBuilderBase<TRequest, TEntity> _builder;

        public CustomSortBuilder<TRequest, TEntity> Custom(
            Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> customSortFunc)
        {
            var builder = new CustomSortBuilder<TRequest, TEntity>(customSortFunc);
            _builder = builder;

            return builder;
        }

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> SortBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            _builder = builder;
            
            return builder.SortBy(entityProperty);
        }

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> SortBy(string entityProperty)
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();
            _builder = builder;

            return builder.SortBy(entityProperty);
        }

        public SwitchSortBuilder<TRequest, TEntity, TValue> AsSwitchSort<TValue>(
            Func<TRequest, TValue> getSwitchValue)
            where TValue : class
        {
            var builder = new SwitchSortBuilder<TRequest, TEntity, TValue>(getSwitchValue);
            _builder = builder;

            return builder;
        }

        public SwitchSortBuilder<TRequest, TEntity, TValue> AsSwitchSort<TValue>(string requestProperty)
            where TValue : class
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var requestProp = Expression.PropertyOrField(requestParam, requestProperty);

            var readPropExpr = Expression.Lambda<Func<TRequest, TValue>>(requestProp, requestParam);

            var builder = new SwitchSortBuilder<TRequest, TEntity, TValue>(readPropExpr.Compile());
            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTableSort<TControl>()
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();

            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTableSort<TControl>(
            Func<TRequest, TControl> getControlValue,
            Func<TRequest, SortDirection> getDirectionValue)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(getControlValue, getDirectionValue);

            _builder = builder;

            return builder;
        }
        
        public TableSortBuilder<TRequest, TEntity, TControl> AsTableSort<TControl>(
            Func<TRequest, TControl> getControlValue,
            SortDirection direction = SortDirection.Default)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(getControlValue, direction);

            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTableSort<TControl>(
            string controlProperty, SortDirection directionValue)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(controlProperty, directionValue);

            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTableSort<TControl>(
            string controlProperty, string directionProperty)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(controlProperty, directionProperty);

            _builder = builder;

            return builder;
        }

        internal ISorter Build()
        {
            return _builder?.Build();
        }
    }
}
