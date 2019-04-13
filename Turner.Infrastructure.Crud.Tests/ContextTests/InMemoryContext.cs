using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.Tests.ContextTests
{
    public class InMemoryContext : IEntityContext
    {
        private static readonly Dictionary<Type, Tuple<IInMemorySet, IList>> _sets
            = new Dictionary<Type, Tuple<IInMemorySet, IList>>();
        
        public Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
            => Task.FromResult(0);
        
        public EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (!_sets.TryGetValue(typeof(TEntity), out var set))
            {
                var dataList = new List<TEntity>();

                _sets[typeof(TEntity)] = new Tuple<IInMemorySet, IList>(
                    new InMemorySet<TEntity>(this, dataList, new InMemorySetOperator()),
                    dataList);
            }

            return _sets[typeof(TEntity)].Item1 as InMemorySet<TEntity>;
        }
    }
}
