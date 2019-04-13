using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.Tests.ContextTests
{
    public interface IInMemorySet
    {
    }

    public class InMemorySet<TEntity> : EntitySet<TEntity>, IInMemorySet
        where TEntity : class
    {
        public List<TEntity> Items { get; private set; }

        public int Id { get; set; } = 1;
        
        public InMemorySet(InMemoryContext context, List<TEntity> items, InMemorySetOperator setOperator)
            : base(CreateEntityQueryable(items), setOperator, setOperator)
        {
            Items = items;
        }

        private static EntityQueryable<TEntity> CreateEntityQueryable(List<TEntity> items)
        {
            var data = items.AsQueryable();
            var provider = new InMemoryQueryProvider(data.Provider);
            
            return new EntityQueryable<TEntity>(provider, Expression.Constant(data));
        }
    }
}
