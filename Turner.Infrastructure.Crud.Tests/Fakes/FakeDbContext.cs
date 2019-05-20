using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class FakeDbContext : DbContext
    {
        public FakeDbContext(DbContextOptions options)
            : base(options)
        {
        }
        
        public async Task Clear()
        {
            Set<User>().RemoveRange(await Set<User>().ToArrayAsync());
            Set<UserClaim>().RemoveRange(await Set<UserClaim>().ToArrayAsync());
            Set<Site>().RemoveRange(await Set<Site>().ToArrayAsync());
            Set<NonEntity>().RemoveRange(await Set<NonEntity>().ToArrayAsync());
            Set<HookEntity>().RemoveRange(await Set<HookEntity>().ToArrayAsync());

            await SaveChangesAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(x => x.Id).HasValueGenerator<ResettableValueGenerator>();
            modelBuilder.Entity<UserClaim>().Property(x => x.Id).HasValueGenerator<ResettableValueGenerator>();
            modelBuilder.Entity<Site>().Property(x => x.Id).HasValueGenerator<ResettableValueGenerator>();
            modelBuilder.Entity<NonEntity>().Property(x => x.Id).HasValueGenerator<ResettableValueGenerator>();
            modelBuilder.Entity<HookEntity>().Property(x => x.Id).HasValueGenerator<ResettableValueGenerator>();
        }
    }

    public static class DbContextExtensions
    {
        public static void ResetValueGenerators(this DbContext context)
        {
            var cache = context.GetService<IValueGeneratorCache>();

            foreach (var keyProperty in context.Model.GetEntityTypes()
                .Select(e => e.FindPrimaryKey().Properties[0])
                .Where(p => p.ClrType == typeof(int) && p.ValueGenerated == ValueGenerated.OnAdd))
            {
                var generator = (ResettableValueGenerator)cache.GetOrAdd(
                    keyProperty,
                    keyProperty.DeclaringEntityType,
                    (p, e) => new ResettableValueGenerator());

                generator.Reset();
            }
        }
    }

    public class ResettableValueGenerator : ValueGenerator<int>
    {
        private int _current;

        public override bool GeneratesTemporaryValues => false;

        public override int Next(EntityEntry entry)
            => Interlocked.Increment(ref _current);

        public void Reset() => _current = 0;
    }
}
