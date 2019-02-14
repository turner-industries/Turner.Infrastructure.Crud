using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Sort
{
    internal abstract class SwitchSortClause<TRequest, TEntity>
        where TEntity : class
    {
        public SortDirection Direction { get; set; }

        internal abstract void Build(SwitchSortOperation<TRequest, TEntity> operation);
    }

    internal class SwitchSortClause<TRequest, TEntity, TProp> : SwitchSortClause<TRequest, TEntity>
        where TEntity : class
    {
        public Expression<Func<TEntity, TProp>> Clause { get; set; }

        internal override void Build(SwitchSortOperation<TRequest, TEntity> operation)
        {
            operation.AddSort(Clause, Direction);
        }
    }

    public class SwitchSortBuilder<TRequest, TEntity, TValue>
        : SortBuilderBase<TRequest, TEntity>
        where TEntity : class
        where TValue : class
    {
        private readonly Func<TRequest, TValue> _getSwitchValue;

        private readonly Dictionary<TValue, SwitchSortOperationBuilder<TRequest, TEntity, TValue>> _cases
            = new Dictionary<TValue, SwitchSortOperationBuilder<TRequest, TEntity, TValue>>();

        private SwitchSortOperationBuilder<TRequest, TEntity, TValue> _default;

        public SwitchSortBuilder(Func<TRequest, TValue> getSwitchValue)
        {
            _getSwitchValue = getSwitchValue;
        }

        internal override ISorterFactory Build()
        {
            var sorter = new SwitchSorter<TRequest, TEntity, TValue>(o => _getSwitchValue((TRequest) o));

            foreach (var (k, v) in _cases)
                sorter.Case(k, v.Build());

            if (_default != null)
                sorter.Default(_default.Build());

            return InstanceSorterFactory.From(sorter);
        }
        
        public SwitchSortOperationBuilder<TRequest, TEntity, TValue> ForCase(TValue value)
        {
            var builder = new SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue>(this);
            _cases[value] = builder;

            return builder;
        }

        public SwitchSortOperationBuilder<TRequest, TEntity, TValue> ForDefault()
        {
            var builder = new SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue>(this);
            _default = builder;

            return builder;
        }
    }

    public abstract class SwitchSortOperationBuilder<TRequest, TEntity, TValue>
        where TEntity : class
        where TValue : class
    {
        protected readonly SwitchSortBuilder<TRequest, TEntity, TValue> ParentBuilder;

        protected readonly List<SwitchSortClauseBuilder<TRequest, TEntity, TValue>> Clauses
            = new List<SwitchSortClauseBuilder<TRequest, TEntity, TValue>>();

        protected SwitchSortOperationBuilder(SwitchSortBuilder<TRequest, TEntity, TValue> parent)
        {
            ParentBuilder = parent ?? throw new ArgumentException(nameof(parent));
        }

        internal SwitchSortOperation<TRequest, TEntity> Build()
        {
            var operation = new SwitchSortOperation<TRequest, TEntity>();
            Clauses.ForEach(builder => builder.Clause.Build(operation));

            return operation;
        }

        public ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> SortBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
        {
            var clauseBuilder = ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>
                .From(this as SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue>, entityProperty);

            Clauses.Add(clauseBuilder);

            return clauseBuilder;
        }
        
        public ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> SortBy(string entityProperty)
        {
            var clauseBuilder = ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>
                .From(this as SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue>, entityProperty);

            Clauses.Add(clauseBuilder);

            return clauseBuilder;
        }
    }

    public class SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue>
        : SwitchSortOperationBuilder<TRequest, TEntity, TValue>
        where TEntity : class
        where TValue : class
    {
        public SwitchSortContinuationOperationBuilder(SwitchSortBuilder<TRequest, TEntity, TValue> parent)
            : base(parent)
        {
        }

        public ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> ThenBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
        {
            var clauseBuilder = ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>
                .From(this, entityProperty);

            Clauses.Add(clauseBuilder);

            return clauseBuilder;
        }

        public ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> ThenBy(string entityProperty)
        {
            var clauseBuilder = ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>
                .From(this, entityProperty);

            Clauses.Add(clauseBuilder);

            return clauseBuilder;
        }
        
        public SwitchSortOperationBuilder<TRequest, TEntity, TValue> ForCase(TValue value)
            => ParentBuilder.ForCase(value);

        public SwitchSortOperationBuilder<TRequest, TEntity, TValue> ForDefault()
            => ParentBuilder.ForDefault();
    }

    public class SwitchSortClauseBuilder<TRequest, TEntity, TValue>
        where TEntity : class
        where TValue : class
    {
        private SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue> _parentBuilder;

        internal SwitchSortClause<TRequest, TEntity> Clause { get; set; }

        public SwitchSortClauseBuilder(SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue> parent)
        {
            _parentBuilder = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> ThenBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
            => _parentBuilder.ThenBy(entityProperty);

        public ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> ThenBy(string entityProperty)
            => _parentBuilder.ThenBy(entityProperty);
        
        public SwitchSortOperationBuilder<TRequest, TEntity, TValue> ForCase(TValue value)
            => _parentBuilder.ForCase(value);

        public SwitchSortOperationBuilder<TRequest, TEntity, TValue> ForDefault()
            => _parentBuilder.ForDefault();
    }

    public class ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>
        : SwitchSortClauseBuilder<TRequest, TEntity, TValue>
        where TEntity : class
        where TValue : class
    {
        public ConfigurableSwitchSortClauseBuilder(
            SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue> parent)
            : base(parent)
        {
        }

        internal static ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> From<TProp>(
            SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue> parent,
            Expression<Func<TEntity, TProp>> entityProperty)
        {
            var clauseBuilder = new ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>(parent)
            {
                Clause = new SwitchSortClause<TRequest, TEntity, TProp>
                {
                    Direction = SortDirection.Default,
                    Clause = entityProperty
                }
            };

            clauseBuilder.Clause.Direction = SortDirection.Default;

            return clauseBuilder;
        }

        internal static ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> From(
            SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue> parent, string entityProperty)
        {
            var entityParam = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityParam, entityProperty)
                ?? throw new ArgumentException(nameof(entityProperty));

            var fwdMethodInfo = typeof(ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>)
                .GetMethod("ForwardFrom", BindingFlags.Static | BindingFlags.NonPublic);

            var fwdMethod = fwdMethodInfo.MakeGenericMethod(entityProp.Type);

            return (ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue>)
                fwdMethod.Invoke(null, new object[] { parent, entityProperty });
        }

        private static ConfigurableSwitchSortClauseBuilder<TRequest, TEntity, TValue> ForwardFrom<TProp>(
            SwitchSortContinuationOperationBuilder<TRequest, TEntity, TValue> parent,
            string entityProperty)
        {
            var entityExpr = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityExpr, entityProperty);
            var readPropExpr = Expression.Lambda<Func<TEntity, TProp>>(entityProp, entityExpr);

            return From(parent, readPropExpr);
        }

        public SwitchSortClauseBuilder<TRequest, TEntity, TValue> Ascending()
        {
            Clause.Direction = SortDirection.Ascending;
            return this;
        }

        public SwitchSortClauseBuilder<TRequest, TEntity, TValue> Descending()
        {
            Clause.Direction = SortDirection.Descending;
            return this;
        }
    }
}
