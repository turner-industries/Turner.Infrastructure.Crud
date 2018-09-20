using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public abstract class SortOperationBuilder<TEntity>
        where TEntity : class
    {
        internal abstract ISortOperation Build();
        internal abstract void Extend(SortOperation operation);
    }

    public abstract class SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        internal abstract List<ISorter> Build();
    }

    public class SortClauseBuilder<TEntity, TKey>
    {
        public Expression<Func<TEntity, TKey>> Clause { get; set; }
        public bool Ascending { get; set; } = true;
    }    


    public class CaseSortOperationBuilder<TRequest, TEntity, TKey, TValue>
        : SortOperationBuilder<TEntity>
        where TEntity : class
    {
        private readonly SwitchSortBuilder<TRequest, TValue, TEntity> _parentBuilder;
        protected readonly SortClauseBuilder<TEntity, TKey> ClauseBuilder;
        protected SortOperationBuilder<TEntity> SecondaryBuilder;

        public CaseSortOperationBuilder(
            SwitchSortBuilder<TRequest, TValue, TEntity> parentBuilder,
            Expression<Func<TEntity, TKey>> clause)
        {
            _parentBuilder = parentBuilder;

            ClauseBuilder = new SortClauseBuilder<TEntity, TKey>
            {
                Clause = clause
            };
        }
        
        public ConfigurableCaseSortOperationBuilder<TRequest, TEntity, TThenKey, TValue> ThenBy<TThenKey>(
            Expression<Func<TEntity, TThenKey>> clause)
        {
            var builder = new ConfigurableCaseSortOperationBuilder<TRequest, TEntity, TThenKey, TValue>(_parentBuilder, clause);
            SecondaryBuilder = builder;

            return builder;
        }

        public SwitchCaseBuilder<TRequest, TValue, TEntity> When(TValue value)
            => _parentBuilder.ForCase(value);

        public SwitchCaseBuilder<TRequest, TValue, TEntity> Otherwise()
            => _parentBuilder.ForDefault();

        internal override ISortOperation Build()
        {
            var sortOperation = new SortOperation();

            sortOperation.SetPrimary(ClauseBuilder.Clause, ClauseBuilder.Ascending);
            SecondaryBuilder?.Extend(sortOperation);

            return sortOperation;
        }

        internal override void Extend(SortOperation operation)
        {
            operation.AddSecondary(ClauseBuilder.Clause, ClauseBuilder.Ascending);
            SecondaryBuilder?.Extend(operation);
        }
    }

    public class ConfigurableCaseSortOperationBuilder<TRequest, TEntity, TKey, TValue>
        : CaseSortOperationBuilder<TRequest, TEntity, TKey, TValue>
        where TEntity : class
    {
        public ConfigurableCaseSortOperationBuilder(
            SwitchSortBuilder<TRequest, TValue, TEntity> parentBuilder,
            Expression<Func<TEntity, TKey>> clause)
            : base(parentBuilder, clause)
        { }

        public CaseSortOperationBuilder<TRequest, TEntity, TKey, TValue> Ascending()
        {
            ClauseBuilder.Ascending = true;
            return this;
        }

        public CaseSortOperationBuilder<TRequest, TEntity, TKey, TValue> Descending()
        {
            ClauseBuilder.Ascending = false;
            return this;
        }
    }

    public class SwitchCaseBuilder<TRequest, TValue, TEntity>
        where TEntity : class
    {
        private readonly SwitchSortBuilder<TRequest, TValue, TEntity> _parentBuilder;

        private SortOperationBuilder<TEntity> _operationBuilder;

        public SwitchCaseBuilder(SwitchSortBuilder<TRequest, TValue, TEntity> parent)
        {
            _parentBuilder = parent;
        }
        
        public ConfigurableCaseSortOperationBuilder<TRequest, TEntity, TKey, TValue>
            SortBy<TKey>(Expression<Func<TEntity, TKey>> clause)
        {
            var builder = new ConfigurableCaseSortOperationBuilder<TRequest, TEntity, TKey, TValue>(_parentBuilder, clause);
            _operationBuilder = builder;

            return builder;
        }

        internal ISortOperation Build()
            => _operationBuilder.Build();
    }

    public class SwitchSortBuilder<TRequest, TValue, TEntity>
        : SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly Func<object, TValue> _getSwitchValue;
        private readonly Dictionary<TValue, SwitchCaseBuilder<TRequest, TValue, TEntity>> _caseBuilders
            = new Dictionary<TValue, SwitchCaseBuilder<TRequest, TValue, TEntity>>();

        private SwitchCaseBuilder<TRequest, TValue, TEntity> _defaultBuilder;

        public SwitchSortBuilder(Func<TRequest, TValue> getSwitchValue)
        {
            _getSwitchValue = request => getSwitchValue((TRequest) request);
        }

        public static SwitchSortBuilder<TRequest, TValue, TEntity> From(
            Func<TRequest, TValue> getSwitchValue)
        {
            return new SwitchSortBuilder<TRequest, TValue, TEntity>(getSwitchValue);
        }

        public SwitchCaseBuilder<TRequest, TValue, TEntity> ForCase(TValue value)
        {
            var builder = new SwitchCaseBuilder<TRequest, TValue, TEntity>(this);

            _caseBuilders[value] = builder;

            return builder;
        }

        public SwitchCaseBuilder<TRequest, TValue, TEntity> ForDefault()
        {
            // TODO: Finish default
            var builder = new SwitchCaseBuilder<TRequest, TValue, TEntity>(this);

            _defaultBuilder = builder;

            return builder;
        }

        internal override List<ISorter> Build()
        {
            var sorter = new SwitchSorter<TValue>(
                _getSwitchValue,
                _defaultBuilder.Build());

            foreach (var (value, builder) in _caseBuilders)
                sorter.Case(value, builder.Build());

            return new List<ISorter> { sorter };
        }
    }
    

    public class SimpleSortOperationBuilder<TRequest, TEntity, TKey, TSortBuilder>
        : SortOperationBuilder<TEntity>
        where TEntity : class
        where TSortBuilder : SortBuilderBase<TRequest, TEntity>
    {
        protected readonly SortClauseBuilder<TEntity, TKey> ClauseBuilder;
        protected Func<TRequest, bool> ClausePredicate;
        protected SortOperationBuilder<TEntity> SecondaryBuilder;
        
        public SimpleSortOperationBuilder(Expression<Func<TEntity, TKey>> clause)
        {
            ClauseBuilder = new SortClauseBuilder<TEntity, TKey>
            {
                Clause = clause
            };
        }
        
        internal TSortBuilder ParentBuilder { get; set; }
        internal Func<TRequest, bool> Predicate => ClausePredicate;
        
        public ConfigurableSimpleSortOperationBuilder<TRequest, TEntity, TThenKey, TSortBuilder>
            ThenBy<TThenKey>(Expression<Func<TEntity, TThenKey>> clause)
        {
            var builder = new ConfigurableSimpleSortOperationBuilder<TRequest, TEntity, TThenKey, TSortBuilder>(clause)
            {
                ParentBuilder = ParentBuilder
            };

            SecondaryBuilder = builder;
            
            return builder;
        }
        
        public TSortBuilder When(Func<TRequest, bool> predicate)
        {
            ClausePredicate = predicate;
            return ParentBuilder;
        }

        internal override ISortOperation Build()
        {
            var sortOperation = new SortOperation();

            sortOperation.SetPrimary(ClauseBuilder.Clause, ClauseBuilder.Ascending);
            
            SecondaryBuilder?.Extend(sortOperation);

            return sortOperation;
        }

        internal override void Extend(SortOperation operation)
        {
            operation.AddSecondary(ClauseBuilder.Clause, ClauseBuilder.Ascending);
            SecondaryBuilder?.Extend(operation);
        }
    }
    
    public class ConfigurableSimpleSortOperationBuilder<TRequest, TEntity, TKey, TSortBuilder>
        : SimpleSortOperationBuilder<TRequest, TEntity, TKey, TSortBuilder>
        where TEntity : class
        where TSortBuilder : SortBuilderBase<TRequest, TEntity>
    {
        public ConfigurableSimpleSortOperationBuilder(Expression<Func<TEntity, TKey>> clause)
            : base(clause)
        { }

        public SimpleSortOperationBuilder<TRequest, TEntity, TKey, TSortBuilder> Ascending()
        {
            ClauseBuilder.Ascending = true;
            return this;
        }

        public SimpleSortOperationBuilder<TRequest, TEntity, TKey, TSortBuilder> Descending()
        {
            ClauseBuilder.Ascending = false;
            return this;
        }
    }

    public class SimpleSortBuilder<TRequest, TEntity>
        : SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly List<SortOperationBuilder<TEntity>> _operationBuilders
            = new List<SortOperationBuilder<TEntity>>();

        public static SimpleSortBuilder<TRequest, TEntity> From<TKey>(
            Expression<Func<TEntity, TKey>> clause,
            out ConfigurableSimpleSortOperationBuilder<TRequest, TEntity, TKey, SimpleSortBuilder<TRequest, TEntity>> operationBuilder)
        {
            var sortBuilder = new SimpleSortBuilder<TRequest, TEntity>();
            operationBuilder = sortBuilder.SortBy(clause);

            return sortBuilder;
        }

        internal override List<ISorter> Build()
        {
            var sorters = new List<ISorter>();

            foreach (var builder in _operationBuilders)
            {
                var sortOperation = builder.Build();
                var sorter = new SimpleSorter(sortOperation);
                
                // TODO: sorter.Predicate

                sorters.Add(sorter);
            }

            return sorters;
        }

        public ConfigurableSimpleSortOperationBuilder<TRequest, TEntity, TKey, SimpleSortBuilder<TRequest, TEntity>>
            SortBy<TKey>(Expression<Func<TEntity, TKey>> clause)
        {
            var operationBuilder = new ConfigurableSimpleSortOperationBuilder<TRequest, TEntity, TKey, SimpleSortBuilder<TRequest, TEntity>>(clause)
            {
                ParentBuilder = this
            };

            _operationBuilders.Add(operationBuilder);

            return operationBuilder;
        }
    }


    public class SortBuilder<TRequest, TEntity>
        where TEntity : class
    {
        private SortBuilderBase<TRequest, TEntity> _sortBuilder;
        
        public ConfigurableSimpleSortOperationBuilder<TRequest, TEntity, TKey, SimpleSortBuilder<TRequest, TEntity>>
            SortBy<TKey>(Expression<Func<TEntity, TKey>> clause)
        {
            _sortBuilder = SimpleSortBuilder<TRequest, TEntity>.From(clause, out var operationBuilder);
            return operationBuilder;
        }

        public SwitchSortBuilder<TRequest, TValue, TEntity> SwitchSortOn<TValue>(
            Func<TRequest, TValue> getSwitchValue)
        {
            var builder = SwitchSortBuilder<TRequest, TValue, TEntity>.From(getSwitchValue);
            _sortBuilder = builder;

            return builder;
        }

        internal List<ISorter> Build()
        {
            return _sortBuilder?.Build();
        }
    }
}
