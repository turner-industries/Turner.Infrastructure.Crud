﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Sort
{
    public class TableSortBuilder<TRequest, TEntity, TControl>
        : SortBuilderBase<TRequest, TEntity>
        where TEntity : class
        where TControl : class
    {
        private readonly List<Func<TRequest, TControl>> _controls =
            new List<Func<TRequest, TControl>>();

        public readonly List<Func<TRequest, SortDirection>> _directions =
            new List<Func<TRequest, SortDirection>>();

        private readonly Dictionary<TControl, IEntityExpressionHolder> _columns =
            new Dictionary<TControl, IEntityExpressionHolder>();

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(
            Func<TRequest, TControl> getControlValue,
            Func<TRequest, SortDirection> getDirectionValue)
        {
            _controls.Add(getControlValue ?? throw new ArgumentNullException(nameof(getControlValue)));
            _directions.Add(getDirectionValue ?? throw new ArgumentNullException(nameof(getDirectionValue)));

            return this;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(
            Func<TRequest, TControl> getControlValue,
            SortDirection directionValue)
        {
            return WithControl(getControlValue, r => directionValue);
        }

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(string controlProperty, SortDirection directionValue)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var controlProp = Expression.PropertyOrField(requestParam, controlProperty)
                ?? throw new ArgumentException(nameof(controlProperty));

            var readPropExpr = Expression.Lambda<Func<TRequest, TControl>>(controlProp, requestParam);

            return WithControl(readPropExpr.Compile(), directionValue);
        }

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(string controlProperty, string directionProperty)
        {
            var requestExpr = Expression.Parameter(typeof(TRequest));

            var controlProp = Expression.PropertyOrField(requestExpr, controlProperty)
                ?? throw new ArgumentException(nameof(controlProperty));

            var readPropExpr = Expression.Lambda<Func<TRequest, TControl>>(controlProp, requestExpr);

            var dirProp = Expression.PropertyOrField(requestExpr, directionProperty)
                ?? throw new ArgumentException(nameof(directionProperty));
            
            var fwdMethodInfo = typeof(TableSortBuilder<TRequest, TEntity, TControl>)
                .GetMethod("ForwardWithControl", BindingFlags.Instance | BindingFlags.NonPublic);

            var fwdMethod = fwdMethodInfo.MakeGenericMethod(dirProp.Type);

            return (TableSortBuilder<TRequest, TEntity, TControl>)
                fwdMethod.Invoke(this, new object[] { readPropExpr.Compile(), directionProperty });
        }

        private TableSortBuilder<TRequest, TEntity, TControl> ForwardWithControl<TDirection>(Func<TRequest, TControl> getControlValue, string directionProperty)
        {
            var requestExpr = Expression.Parameter(typeof(TRequest));
            var dirProp = Expression.PropertyOrField(requestExpr, directionProperty);
            var readDirExpr = Expression.Lambda<Func<TRequest, TDirection>>(dirProp, requestExpr);
            var readDir = readDirExpr.Compile();

            return WithControl(getControlValue,
                request =>
                {
                    if (Enum.TryParse<SortDirection>(Convert.ToString(readDir(request)), out var direction))
                        return direction;

                    return SortDirection.Default;
                });
        }
        
        public TableSortBuilder<TRequest, TEntity, TControl> WithColumn<TKey>(
            TControl controlValue,
            Expression<Func<TEntity, TKey>> clause)
        {
            if (controlValue == null)
                throw new ArgumentNullException(nameof(controlValue));

            _columns[controlValue] = new EntityExpressionHolder<TKey>(
                clause ?? throw new ArgumentNullException(nameof(clause)));

            return this;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> WithColumn(TControl controlValue, string entityProperty)
        {
            if (controlValue == null)
                throw new ArgumentNullException(nameof(controlValue));

            var entityParam = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityParam, entityProperty)
                ?? throw new ArgumentException(nameof(entityProperty));

            var fwdMethodInfo = typeof(TableSortBuilder<TRequest, TEntity, TControl>)
                .GetMethod("ForwardWithColumn", BindingFlags.Instance | BindingFlags.NonPublic);

            var fwdMethod = fwdMethodInfo.MakeGenericMethod(entityProp.Type);

            return (TableSortBuilder<TRequest, TEntity, TControl>)
                fwdMethod.Invoke(this, new object[] { controlValue, entityProperty });
        }
        
        private TableSortBuilder<TRequest, TEntity, TControl> ForwardWithColumn<TKey>(
            TControl controlValue, string entityProperty)
        {
            var entityParam = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityParam, entityProperty);
            var readPropExpr = Expression.Lambda<Func<TEntity, TKey>>(entityProp, entityParam);

            return WithColumn(controlValue, readPropExpr);
        }

        internal override ISorter Build()
        {
            var sorter = new TableSorter<TControl>();

            _controls
                .Zip(_directions, (c, d) => new { Control = c, Direction = d })
                .ToList()
                .ForEach(z => sorter.AddControl(z.Control, z.Direction));

            foreach (var (k, v) in _columns)
                v.Build(sorter, k);

            return sorter;
        }

        private interface IEntityExpressionHolder
        {
            void Build(TableSorter<TControl> sorter, TControl controlValue);
        }

        private class EntityExpressionHolder<TKey> : IEntityExpressionHolder
        {
            private Expression<Func<TEntity, TKey>> Expression { get; set; }

            public EntityExpressionHolder(Expression<Func<TEntity, TKey>> expr)
            {
                Expression = expr;
            }

            public void Build(TableSorter<TControl> sorter, TControl controlValue)
            {
                sorter.AddColumn(controlValue, Expression);
            }
        }
    }
}