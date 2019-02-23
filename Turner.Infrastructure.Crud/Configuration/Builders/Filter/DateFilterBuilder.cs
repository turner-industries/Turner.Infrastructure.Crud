using System;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Filter
{
    public class DateFilterBuilder<TRequest, TEntity>
        : FilterBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly Expression<Func<TEntity, DateTime>> _entityDateExpr;
        private readonly bool _isInclusionFilter;

        private BasicFilterBuilder<TRequest, TEntity> _configuredBuilder;

        public DateFilterBuilder(Expression<Func<TEntity, DateTime>> entityDateExpr, bool isInclusionFilter)
        {
            _entityDateExpr = entityDateExpr;
            _isInclusionFilter = isInclusionFilter;
        }
        
        public BasicFilterBuilder<TRequest, TEntity> Before(
            Expression<Func<TRequest, DateTime>> requestDateExpr)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Invoke(requestDateExpr, requestParam);
            var entityValue = Expression.Invoke(_entityDateExpr, entityParam);
            
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(_isInclusionFilter
                ? Expression.LessThan(entityValue, requestValue)
                : Expression.GreaterThanOrEqual(entityValue, requestValue),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> Before(DateTime value)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Constant(value, typeof(DateTime));
            var entityValue = Expression.Invoke(_entityDateExpr, entityParam);
            
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(_isInclusionFilter
                ? Expression.LessThan(entityValue, requestValue)
                : Expression.GreaterThanOrEqual(entityValue, requestValue),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> After(
            Expression<Func<TRequest, DateTime>> requestDateExpr)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Invoke(requestDateExpr, requestParam);
            var entityValue = Expression.Invoke(_entityDateExpr, entityParam);
            
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(_isInclusionFilter
                ? Expression.GreaterThan(entityValue, requestValue)
                : Expression.LessThanOrEqual(entityValue, requestValue),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> After(DateTime value)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Constant(value, typeof(DateTime));
            var entityValue = Expression.Invoke(_entityDateExpr, entityParam);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(_isInclusionFilter
                ? Expression.GreaterThan(entityValue, requestValue)
                : Expression.LessThanOrEqual(entityValue, requestValue),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> On(
            Expression<Func<TRequest, DateTime>> requestDateExpr)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Invoke(requestDateExpr, requestParam);
            var entityValue = Expression.Invoke(_entityDateExpr, entityParam);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(_isInclusionFilter
                ? Expression.Equal(entityValue, requestValue)
                : Expression.NotEqual(entityValue, requestValue),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> On(DateTime value)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Constant(value, typeof(DateTime));
            var entityValue = Expression.Invoke(_entityDateExpr, entityParam);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(_isInclusionFilter
                ? Expression.Equal(entityValue, requestValue)
                : Expression.NotEqual(entityValue, requestValue),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> Within(
            DateTime minValue,
            DateTime maxValue,
            bool minInclusive = true,
            bool maxInclusive = true)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));

            return CreateWithinFilter(requestParam,
                Expression.Constant(minValue, typeof(DateTime)),
                Expression.Constant(maxValue, typeof(DateTime)),
                minInclusive,
                maxInclusive);
        }

        public BasicFilterBuilder<TRequest, TEntity> Within(
            Expression<Func<TRequest, DateTime>> requestMinDateExpr,
            DateTime maxValue,
            bool minInclusive = true,
            bool maxInclusive = true)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));

            return CreateWithinFilter(requestParam,
                Expression.Invoke(requestMinDateExpr, requestParam),
                Expression.Constant(maxValue, typeof(DateTime)),
                minInclusive,
                maxInclusive);
        }

        public BasicFilterBuilder<TRequest, TEntity> Within(
            DateTime minValue,
            Expression<Func<TRequest, DateTime>> requestMaxDateExpr,
            bool minInclusive = true,
            bool maxInclusive = true)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));

            return CreateWithinFilter(requestParam,
                Expression.Constant(minValue, typeof(DateTime)),
                Expression.Invoke(requestMaxDateExpr, requestParam),
                minInclusive,
                maxInclusive);
        }

        public BasicFilterBuilder<TRequest, TEntity> Within(
            Expression<Func<TRequest, DateTime>> requestDateMinExpr,
            Expression<Func<TRequest, DateTime>> requestDateMaxExpr,
            bool minInclusive = true,
            bool maxInclusive = true)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
      
            return CreateWithinFilter(requestParam, 
                Expression.Invoke(requestDateMinExpr, requestParam),
                Expression.Invoke(requestDateMaxExpr, requestParam), 
                minInclusive, 
                maxInclusive);
        }

        internal override IFilterFactory Build()
        {
            return _configuredBuilder?.Build();
        }

        private BasicFilterBuilder<TRequest, TEntity> CreateWithinFilter(
            ParameterExpression requestParam, 
            Expression minValueExpr,
            Expression maxValueExpr,
            bool minInclusive, 
            bool maxInclusive)
        {
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var entityValue = Expression.Invoke(_entityDateExpr, entityParam);

            var minClause = minInclusive
                ? Expression.GreaterThanOrEqual(entityValue, minValueExpr)
                : (Expression)Expression.GreaterThan(entityValue, minValueExpr);

            var maxClause = maxInclusive
                ? Expression.LessThanOrEqual(entityValue, maxValueExpr)
                : (Expression)Expression.LessThan(entityValue, maxValueExpr);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(_isInclusionFilter
                ? (Expression)Expression.And(minClause, maxClause)
                : Expression.Not(Expression.And(minClause, maxClause)), entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }
        
        private Expression<Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>>> CreateFilter(
            ParameterExpression requestExpr,
            ParameterExpression queryableExpr,
            Expression<Func<TEntity, bool>> whereClause)
        {
            var whereInfo = typeof(Queryable)
                .GetMethods()
                .Single(x => x.Name == "Where"
                    && x.GetParameters().Length == 2
                    && x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1
                    && x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TEntity));

            return Expression.Lambda<Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>>>(
                Expression.Call(whereInfo, queryableExpr, whereClause),
                requestExpr,
                queryableExpr);
        }
    }
}
