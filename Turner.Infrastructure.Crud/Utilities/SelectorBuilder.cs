using System;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Utilities
{
    public class SelectorBuilder<TRequest, TEntity>
        where TEntity : class
    {
        public Func<TRequest, Expression<Func<TEntity, bool>>> Build<TRequestKey, TEntityKey>(
                Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
                Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
        {
            return request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.Invoke(requestKeyExpr, rParamExpr);
                var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            };
        }

        public Func<TRequest, Expression<Func<TEntity, bool>>> Build(
            string requestKeyProperty, string entityKeyProperty)
        {
            return request =>
            {
                var eParamExpr = Expression.Parameter(typeof(TEntity));
                var eKeyExpr = Expression.Property(eParamExpr, entityKeyProperty);
                var rParamExpr = Expression.Constant(request);
                var rKeyExpr = Expression.Property(rParamExpr, requestKeyProperty);
                var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

                return Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            };
        }
    }
}
