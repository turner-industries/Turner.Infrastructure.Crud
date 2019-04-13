using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkEntitySet<TEntity> : EntitySet<TEntity>
        where TEntity : class
    {
        public DbSet<TEntity> Set { get; }

        public EntityFrameworkEntitySet(DbSet<TEntity> set, 
            ISingleSetOperator singleSetOperator,
            IBulkSetOperator bulkSetOperator)
            : base(EntityFrameworkEntityQueryable<TEntity>.From(set), singleSetOperator, bulkSetOperator)
        {
            Set = set;
        }
    }
}
