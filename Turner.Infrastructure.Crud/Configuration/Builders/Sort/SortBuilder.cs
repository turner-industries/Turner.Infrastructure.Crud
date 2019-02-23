using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Sort
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public abstract class SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        internal abstract ISorterFactory Build();
    }

    public class SortBuilder<TRequest, TEntity>
        where TEntity : class
    {
        private SortBuilderBase<TRequest, TEntity> _builder;

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

        public SwitchSortBuilder<TRequest, TEntity, TValue> AsSwitch<TValue>(
            Func<TRequest, TValue> getSwitchValue)
            where TValue : class
        {
            var builder = new SwitchSortBuilder<TRequest, TEntity, TValue>(getSwitchValue);
            _builder = builder;

            return builder;
        }

        public SwitchSortBuilder<TRequest, TEntity, TValue> AsSwitch<TValue>(string requestProperty)
            where TValue : class
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var requestProp = Expression.PropertyOrField(requestParam, requestProperty);

            var readPropExpr = Expression.Lambda<Func<TRequest, TValue>>(requestProp, requestParam);

            var builder = new SwitchSortBuilder<TRequest, TEntity, TValue>(readPropExpr.Compile());
            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTable<TControl>()
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();

            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTable<TControl>(
            Func<TRequest, TControl> getControlValue,
            Func<TRequest, SortDirection> getDirectionValue)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(getControlValue, getDirectionValue);

            _builder = builder;

            return builder;
        }
        
        public TableSortBuilder<TRequest, TEntity, TControl> AsTable<TControl>(
            Func<TRequest, TControl> getControlValue,
            SortDirection direction = SortDirection.Default)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(getControlValue, direction);

            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTable<TControl>(
            string controlProperty, SortDirection directionValue)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(controlProperty, directionValue);

            _builder = builder;

            return builder;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> AsTable<TControl>(
            string controlProperty, string directionProperty)
            where TControl : class
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();
            builder.WithControl(controlProperty, directionProperty);

            _builder = builder;

            return builder;
        }

        internal ISorterFactory Build()
        {
            return _builder?.Build();
        }
    }
}
