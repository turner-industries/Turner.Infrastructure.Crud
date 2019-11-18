using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Sort
{
    internal abstract class BasicSortClause<TRequest, TEntity>
        where TEntity : class
    {
        public SortDirection Direction { get; set; }

        internal abstract void Build(BasicSortOperation<TRequest, TEntity> operation);
    }

    internal class BasicSortClause<TRequest, TEntity, TProp> : BasicSortClause<TRequest, TEntity>
        where TEntity : class
    {
        public Expression<Func<TEntity, TProp>> Clause { get; set; }

        internal override void Build(BasicSortOperation<TRequest, TEntity> operation)
        {
            operation.AddSort(Clause, Direction);
        }
    }

    public class BasicSortBuilder<TRequest, TEntity> 
        : SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly List<BasicSortOperationBuilder<TRequest, TEntity>> _operations
            = new List<BasicSortOperationBuilder<TRequest, TEntity>>();

        internal override ISorterFactory Build()
        {
            if (_operations.Count == 0)
            {
                throw new BadConfigurationException(
                    $"Basic sorting was set for request '{typeof(TRequest)}' and entity '{typeof(TEntity)}'" +
                    ", but no sort operation was defined.");
            }

            var instance = new BasicSorter<TRequest, TEntity>(_operations.Select(builder => builder.Build()).ToList());

            return InstanceSorterFactory.From(instance);
        }
        
        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> SortBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
        {
            var operationBuilder = new BasicSortOperationBuilder<TRequest, TEntity>(this);
            
            _operations.Add(operationBuilder);

            return operationBuilder.ThenBy(entityProperty);
        }

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> SortBy(string entityProperty)
        {
            var operationBuilder = new BasicSortOperationBuilder<TRequest, TEntity>(this);

            _operations.Add(operationBuilder);

            return operationBuilder.ThenBy(entityProperty);
        }
    }

    public class BasicSortOperationBuilder<TRequest, TEntity> 
        where TEntity : class
    {
        private readonly BasicSortBuilder<TRequest, TEntity> _parentBuilder;
        private readonly List<BasicSortClauseBuilder<TRequest, TEntity>> _clauses
            = new List<BasicSortClauseBuilder<TRequest, TEntity>>();

        private Func<TRequest, bool> _predicate;

        public BasicSortOperationBuilder(BasicSortBuilder<TRequest, TEntity> parent)
        {
            _parentBuilder = parent ?? throw new ArgumentNullException(nameof(parent));
        }
        
        internal BasicSortOperation<TRequest, TEntity> Build()
        {
            var operation = new BasicSortOperation<TRequest, TEntity>(_predicate);

            _clauses.ForEach(builder => builder.Clause.Build(operation));

            return operation;
        }

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> SortBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
            => _parentBuilder.SortBy(entityProperty);

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> SortBy(string entityProperty)
            => _parentBuilder.SortBy(entityProperty);

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> ThenBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
        {
            var clauseBuilder = ConfigurableBasicSortClauseBuilder<TRequest, TEntity>
                .From(this, entityProperty);

            _clauses.Add(clauseBuilder);

            return clauseBuilder;
        }

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> ThenBy(string entityProperty)
        {
            var entityParam = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityParam, entityProperty);
            
            var fwdMethodInfo = typeof(BasicSortOperationBuilder<TRequest, TEntity>)
                .GetMethod(nameof(ForwardThenBy), BindingFlags.Instance | BindingFlags.NonPublic);

            // ReSharper disable once PossibleNullReferenceException
            var fwdMethod = fwdMethodInfo.MakeGenericMethod(entityProp.Type);

            return (ConfigurableBasicSortClauseBuilder<TRequest, TEntity>) 
                fwdMethod.Invoke(this, new object[] { entityProperty});
        }
        
        private ConfigurableBasicSortClauseBuilder<TRequest, TEntity> ForwardThenBy<TProp>(string entityProperty)
        {
            var entityExpr = Expression.Parameter(typeof(TEntity));
            var entityProp = Expression.PropertyOrField(entityExpr, entityProperty);
            var readPropExpr = Expression.Lambda<Func<TEntity, TProp>>(entityProp, entityExpr);

            return ThenBy(readPropExpr);
        }

        public BasicSortBuilder<TRequest, TEntity> When(Func<TRequest, bool> predicate)
        {
            _predicate = predicate;

            return _parentBuilder;
        }

        public void Otherwise()
        {
            _predicate = r => true;
        }
    }

    public class BasicSortClauseBuilder<TRequest, TEntity>
        where TEntity : class
    {
        private BasicSortOperationBuilder<TRequest, TEntity> _parentBuilder;

        internal BasicSortClause<TRequest, TEntity> Clause { get; set; }

        public BasicSortClauseBuilder(BasicSortOperationBuilder<TRequest, TEntity> parent)
        {
            _parentBuilder = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> ThenBy<TProp>(
            Expression<Func<TEntity, TProp>> entityProperty)
            => _parentBuilder.ThenBy(entityProperty);

        public ConfigurableBasicSortClauseBuilder<TRequest, TEntity> ThenBy(string entityProperty)
            => _parentBuilder.ThenBy(entityProperty);

        public BasicSortBuilder<TRequest, TEntity> When(Func<TRequest, bool> predicate)
            => _parentBuilder.When(predicate);

        public void Otherwise()
            => _parentBuilder.Otherwise();
    }

    public class ConfigurableBasicSortClauseBuilder<TRequest, TEntity>
        : BasicSortClauseBuilder<TRequest, TEntity>
        where TEntity : class
    {
        public ConfigurableBasicSortClauseBuilder(BasicSortOperationBuilder<TRequest, TEntity> parent)
            : base(parent)
        {
        }

        internal static ConfigurableBasicSortClauseBuilder<TRequest, TEntity> From<TProp>(
            BasicSortOperationBuilder<TRequest, TEntity> parent,
            Expression<Func<TEntity, TProp>> entityProperty)
        {
            var clauseBuilder = new ConfigurableBasicSortClauseBuilder<TRequest, TEntity>(parent)
            {
                Clause = new BasicSortClause<TRequest, TEntity, TProp>
                {
                    Direction = SortDirection.Default,
                    Clause = entityProperty
                }
            };

            clauseBuilder.Clause.Direction = SortDirection.Default;

            return clauseBuilder;
        }

        public BasicSortClauseBuilder<TRequest, TEntity> Ascending()
        {
            Clause.Direction = SortDirection.Ascending;
            return this;
        }

        public BasicSortClauseBuilder<TRequest, TEntity> Descending()
        {
            Clause.Direction = SortDirection.Descending;
            return this;
        }
    }
}
