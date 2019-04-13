using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkEntityQueryable<TEntity> : EntityQueryable<TEntity>
        where TEntity : class
    {
        private EntityFrameworkEntityQueryable(IEntityQueryProvider queryProvider, Expression expression)
            : base(queryProvider, expression)
        {
        }

        public static EntityFrameworkEntityQueryable<TEntity> From(DbSet<TEntity> set)
        {
            var queryProvider = set.AsQueryable().Provider;

            return new EntityFrameworkEntityQueryable<TEntity>(
                new EntityFrameworkQueryProvider(queryProvider),
                Expression.Constant(set));
        }
    }
}
