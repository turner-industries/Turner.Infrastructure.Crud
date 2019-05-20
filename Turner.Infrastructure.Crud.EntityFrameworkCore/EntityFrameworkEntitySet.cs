using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkEntitySet<TEntity> : EntitySet<TEntity>
        where TEntity : class
    {
        public DbContext Context { get; }

        public EntityFrameworkEntitySet(DbContext context, IDataAgentFactory dataAgentFactory)
            : base(CreateEntityQueryable(context.Set<TEntity>()), dataAgentFactory)
        {
            Context = context;
        }

        private static EntityQueryable<TEntity> CreateEntityQueryable(DbSet<TEntity> set)
        {
            var queryProvider = set.AsQueryable().Provider;

            return new EntityQueryable<TEntity>(
                new EntityFrameworkQueryProvider(queryProvider),
                Expression.Constant(set));
        }
    }
}
