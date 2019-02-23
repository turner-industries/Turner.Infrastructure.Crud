using System;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Configuration.Builders.Filter
{
    public class StringFilterBuilder<TRequest, TEntity>
        : FilterBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        private readonly Expression<Func<TEntity, string>> _entityStringExpr;
        private readonly bool _isInclusionFilter;

        private BasicFilterBuilder<TRequest, TEntity> _configuredBuilder;

        public StringFilterBuilder(Expression<Func<TEntity, string>> entityStringExpr, bool isInclusionFilter)
        {
            _entityStringExpr = entityStringExpr;
            _isInclusionFilter = isInclusionFilter;
        }

        public BasicFilterBuilder<TRequest, TEntity> StartingWith(
            Expression<Func<TRequest, string>> requestStringExpr)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Invoke(requestStringExpr, requestParam);
            var entityValue = Expression.Invoke(_entityStringExpr, entityParam);

            var searchMethod = typeof(string)
                .GetMethods()
                .Single(x =>
                    x.Name == "StartsWith" &&
                    x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == typeof(string));

            var searchExpr = Expression.Call(entityValue, searchMethod, requestValue);
            
            var whereClause = Expression.Lambda<Func<TEntity, bool>>(
                _isInclusionFilter ? (Expression)searchExpr : Expression.Not(searchExpr),
                entityParam);
            
            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> StartingWith(string value)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Constant(value, typeof(string));
            var entityValue = Expression.Invoke(_entityStringExpr, entityParam);

            var searchMethod = typeof(string)
                .GetMethods()
                .Single(x =>
                    x.Name == "StartsWith" &&
                    x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == typeof(string));

            var searchExpr = Expression.Call(entityValue, searchMethod, requestValue);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(
                _isInclusionFilter ? (Expression)searchExpr : Expression.Not(searchExpr),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> EndingWith(
            Expression<Func<TRequest, string>> requestStringExpr)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Invoke(requestStringExpr, requestParam);
            var entityValue = Expression.Invoke(_entityStringExpr, entityParam);

            var searchMethod = typeof(string)
                .GetMethods()
                .Single(x =>
                    x.Name == "EndsWith" &&
                    x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == typeof(string));

            var searchExpr = Expression.Call(entityValue, searchMethod, requestValue);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(
                _isInclusionFilter ? (Expression)searchExpr : Expression.Not(searchExpr),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> EndingWith(string value)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Constant(value, typeof(string));
            var entityValue = Expression.Invoke(_entityStringExpr, entityParam);

            var searchMethod = typeof(string)
                .GetMethods()
                .Single(x =>
                    x.Name == "EndsWith" &&
                    x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == typeof(string));

            var searchExpr = Expression.Call(entityValue, searchMethod, requestValue);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(
                _isInclusionFilter ? (Expression)searchExpr : Expression.Not(searchExpr),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> Containing(
            Expression<Func<TRequest, string>> requestStringExpr)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Invoke(requestStringExpr, requestParam);
            var entityValue = Expression.Invoke(_entityStringExpr, entityParam);
            
            var searchMethod = typeof(string)
                .GetMethods()
                .Single(x =>
                    x.Name == "Contains" &&
                    x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == typeof(string));

            var searchExpr = Expression.Call(entityValue, searchMethod, requestValue);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(
                _isInclusionFilter ? (Expression)searchExpr : Expression.Not(searchExpr),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        public BasicFilterBuilder<TRequest, TEntity> Containing(string value)
        {
            var requestParam = Expression.Parameter(typeof(TRequest));
            var queryableParam = Expression.Parameter(typeof(IQueryable<TEntity>));
            var entityParam = Expression.Parameter(typeof(TEntity));

            var requestValue = Expression.Constant(value, typeof(string));
            var entityValue = Expression.Invoke(_entityStringExpr, entityParam);

            var searchMethod = typeof(string)
                .GetMethods()
                .Single(x =>
                    x.Name == "Contains" &&
                    x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == typeof(string));

            var searchExpr = Expression.Call(entityValue, searchMethod, requestValue);

            var whereClause = Expression.Lambda<Func<TEntity, bool>>(
                _isInclusionFilter ? (Expression)searchExpr : Expression.Not(searchExpr),
                entityParam);

            var filterFunc = CreateFilter(requestParam, queryableParam, whereClause).Compile();

            _configuredBuilder = new BasicFilterBuilder<TRequest, TEntity>(filterFunc);

            return _configuredBuilder;
        }

        internal override IFilterFactory Build()
        {
            return _configuredBuilder?.Build();
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
