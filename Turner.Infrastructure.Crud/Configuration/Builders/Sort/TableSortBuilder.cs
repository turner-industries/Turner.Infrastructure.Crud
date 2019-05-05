using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Extensions;

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

        private TControl _defaultColumn;

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(
            Func<TRequest, TControl> getControlValue,
            Func<TRequest, SortDirection> getDirectionValue)
        {
            _controls.Add(getControlValue ?? throw new ArgumentNullException(nameof(getControlValue)));
            _directions.Add(getDirectionValue ?? throw new ArgumentNullException(nameof(getDirectionValue)));

            return this;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl<TDirection>(
            Func<TRequest, TControl> getControlValue,
            Func<TRequest, TDirection> getDirectionValue)
            => ForwardWithControl(getControlValue, getDirectionValue);

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(
            Func<TRequest, TControl> getControlValue,
            SortDirection directionValue)
            => WithControl(getControlValue, r => directionValue);

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(
            string controlProperty, 
            string directionProperty)
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
                fwdMethod.Invoke(this, new object[] { readPropExpr.Compile(), Expression.Lambda(dirProp, requestExpr).Compile() });
        }

        public TableSortBuilder<TRequest, TEntity, TControl> WithControl(
            string controlProperty, 
            SortDirection directionValue)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var controlProp = Expression.PropertyOrField(requestParam, controlProperty)
                ?? throw new ArgumentException(nameof(controlProperty));

            var readPropExpr = Expression.Lambda<Func<TRequest, TControl>>(controlProp, requestParam);

            return WithControl(readPropExpr.Compile(), directionValue);
        }

        private TableSortBuilder<TRequest, TEntity, TControl> ForwardWithControl<TDirection>(
            Func<TRequest, TControl> getControlValue, 
            Func<TRequest, TDirection> getDirectionValue)
        {
            _controls.Add(getControlValue ?? throw new ArgumentNullException(nameof(getControlValue)));

            _directions.Add(request =>
            {
                if (Enum.TryParse<SortDirection>(Convert.ToString(getDirectionValue(request)), out var direction))
                    return direction;

                return SortDirection.Default;
            });

            return this;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> OnProperty<TKey>(
            TControl controlValue,
            Expression<Func<TEntity, TKey>> clause)
            => OnProperty(controlValue, clause, false);

        public TableSortBuilder<TRequest, TEntity, TControl> OnProperty<TKey>(
            TControl controlValue,
            Expression<Func<TEntity, TKey>> clause,
            bool asDefault)
        {
            if (controlValue == null)
                throw new ArgumentNullException(nameof(controlValue));

            _columns[controlValue] = new EntityExpressionHolder<TKey>(
                clause ?? throw new ArgumentNullException(nameof(clause)));

            if (asDefault)
                _defaultColumn = controlValue;

            return this;
        }

        public TableSortBuilder<TRequest, TEntity, TControl> OnProperty(
            TControl controlValue, 
            string entityProperty)
            => OnProperty(controlValue, entityProperty, false);

        public TableSortBuilder<TRequest, TEntity, TControl> OnProperty(
            TControl controlValue, 
            string entityProperty,
            bool asDefault)
        {
            if (controlValue == null)
                throw new ArgumentNullException(nameof(controlValue));

            var entityParam = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityParam, entityProperty)
                ?? throw new ArgumentException(nameof(entityProperty));

            var fwdMethodInfo = typeof(TableSortBuilder<TRequest, TEntity, TControl>)
                .GetMethod("ForwardOnProperty", BindingFlags.Instance | BindingFlags.NonPublic);

            var fwdMethod = fwdMethodInfo.MakeGenericMethod(entityProp.Type);

            return (TableSortBuilder<TRequest, TEntity, TControl>)
                fwdMethod.Invoke(this, new object[] { controlValue, entityProperty, asDefault });
        }

        public TableSortBuilder<TRequest, TEntity, TControl> OnAnyProperty()
            => OnAnyProperty(default(TControl));

        public TableSortBuilder<TRequest, TEntity, TControl> OnAnyProperty<TProperty>(Expression<Func<TEntity, TProperty>> defaultProperty)
        {
            if (!(defaultProperty.Body is MemberExpression memberExpression))
                throw new ArgumentException($"Expression '{defaultProperty}' refers to a method, not a property.");
            
            if (!(memberExpression.Member is PropertyInfo propertyInfo))
                throw new ArgumentException($"Expression '{defaultProperty}' refers to a field, not a property.");

            if (typeof(TEntity) != propertyInfo.ReflectedType && !typeof(TEntity).IsSubclassOf(propertyInfo.ReflectedType))
                throw new ArgumentException($"Expression '{defaultProperty}' refers to a property that is not from type {typeof(TEntity)}.");

            var properties = GetSafeEntityProperties();
            var onAnyPropertyMethod = typeof(TableSortBuilder<TRequest, TEntity, TControl>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == "OnAnyProperty" && !x.IsGenericMethodDefinition && x.GetParameters().Length == 1);

            if (typeof(int).IsAssignableFrom(typeof(TControl)))
            {
                return (TableSortBuilder<TRequest, TEntity, TControl>)
                    onAnyPropertyMethod.Invoke(this, new object[] { Array.IndexOf(properties, propertyInfo) });
            }
            else if (typeof(string).IsAssignableFrom(typeof(TControl)))
            {
                return (TableSortBuilder<TRequest, TEntity, TControl>)
                    onAnyPropertyMethod.Invoke(this, new object[] { propertyInfo.Name });
            }

            throw new BadCrudConfigurationException("'OnAnyProperty' may only be used with int and string controls (TControl)");
        }

        public TableSortBuilder<TRequest, TEntity, TControl> OnAnyProperty(TControl defaultValue)
        {
            if (typeof(int).IsAssignableFrom(typeof(TControl)))
            {
                var properties = GetSafeEntityProperties();
                for (int i = 0; i < properties.Length; ++i)
                    AddPropertyColumn(properties[i], Expression.Constant(i));

                var defaultAsInt = defaultValue as int?;
                if (defaultAsInt.Value < 0 || defaultAsInt.Value >= properties.Length)
                {
                    throw new BadCrudConfigurationException(
                        $"The default table sort property index '{defaultValue}' " +
                        $"for request '{typeof(TRequest)}' and entity '{typeof(TEntity)}' " +
                        "is outside of the range of properties for the entity.");
                }

                _defaultColumn = defaultValue;
            }
            else if (typeof(string).IsAssignableFrom(typeof(TControl)))
            {
                var properties = GetSafeEntityProperties();
                foreach (var property in properties)
                    AddPropertyColumn(property, Expression.Constant(property.Name));

                var defaultAsString = defaultValue as string;
                if (defaultAsString != null && !properties.Any(x => x.Name == defaultAsString))
                {
                    throw new BadCrudConfigurationException(
                        $"The default table sort property '{defaultValue}' " +
                        $"for request '{typeof(TRequest)}' and entity '{typeof(TEntity)}' " +
                        "is not a valid property name for the entity.");
                }

                _defaultColumn = defaultValue;
            }
            else
            {
                throw new BadCrudConfigurationException("'OnAnyProperty' may only be used with int and string controls (TControl)");
            }

            return this;
        }

        private PropertyInfo[] GetSafeEntityProperties()
        {
            var properties = typeof(TEntity).GetProperties();
            if (properties.Length == 0)
            {
                throw new BadCrudConfigurationException(
                    $"OnAnyProperty was called for request '{typeof(TRequest)}' and entity '{typeof(TEntity)}'" +
                    ", but no properties are declared for the entity.");
            }

            return properties;
        }

        private TableSortBuilder<TRequest, TEntity, TControl> ForwardOnProperty<TKey>(
            TControl controlValue, 
            string entityProperty, 
            bool asDefault)
        {
            var entityParam = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityParam, entityProperty);
            var readPropExpr = Expression.Lambda<Func<TEntity, TKey>>(entityProp, entityParam);

            return OnProperty(controlValue, readPropExpr, asDefault);
        }

        private void AddPropertyColumn(PropertyInfo property, ConstantExpression key)
        {
            var addInfo = _columns.GetType().GetMethods().Single(x => x.Name == "Add");

            var columnsParam = Expression.Constant(_columns);
            var entityParam = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.Property(entityParam, property);
            var getEntityProp = Expression.Lambda(entityProp, entityParam);
            var exprHolderType = typeof(TableSortBuilder<,,>.EntityExpressionHolder<>)
                .MakeGenericType(typeof(TRequest), typeof(TEntity), typeof(TControl), property.PropertyType);
            var exprHolderCtor = exprHolderType.GetConstructors()[0];
            var exprHolderParam = Expression.New(exprHolderCtor, getEntityProp);
            var addColumn = Expression.Call(columnsParam, addInfo, key, exprHolderParam);

            Expression.Lambda<Action>(addColumn).Compile().Invoke();
        }

        internal override ISorterFactory Build()
        {
            var sorter = new TableSorter<TRequest, TEntity, TControl>(_defaultColumn);

            _controls
                .Zip(_directions, (c, d) => new { Control = c, Direction = d })
                .ToList()
                .ForEach(z => sorter.AddControl(z.Control, z.Direction));

            foreach (var (k, v) in _columns)
                v.Build(sorter, k);

            if (_controls.Count == 0)
            {
                throw new BadCrudConfigurationException(
                    $"Table sorting was set for request '{typeof(TRequest)}' and entity '{typeof(TEntity)}'" +
                    ", but no controls were defined.");
            }

            if (_defaultColumn == null && _columns.Count == 0)
            {
                throw new BadCrudConfigurationException(
                    $"Table sorting was set for request '{typeof(TRequest)}' and entity '{typeof(TEntity)}'" +
                    ", but no table properties were defined.");
            }

            return InstanceSorterFactory.From(sorter);
        }

        private interface IEntityExpressionHolder
        {
            void Build(TableSorter<TRequest, TEntity, TControl> sorter, TControl controlValue);
        }

        private class EntityExpressionHolder<TKey> : IEntityExpressionHolder
        {
            private Expression<Func<TEntity, TKey>> Expression { get; set; }

            public EntityExpressionHolder(Expression<Func<TEntity, TKey>> expr)
            {
                Expression = expr;
            }

            public void Build(TableSorter<TRequest, TEntity, TControl> sorter, TControl controlValue)
            {
                sorter.AddColumn(controlValue, Expression);
            }
        }
    }
}
