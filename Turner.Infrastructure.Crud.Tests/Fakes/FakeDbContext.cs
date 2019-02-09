using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class FakeDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Site> Sites { get; set; }

        public DbSet<NonEntity> NonEntities { get; set; }

        public DbSet<HookEntity> Hooks { get; set; }

        public DbSet<UserClaim> UserClaims { get; set; }

        public FakeDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Site>();
            modelBuilder.Entity<NonEntity>();
            modelBuilder.Entity<HookEntity>();
            modelBuilder.Entity<UserClaim>();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var deletedEntries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in deletedEntries)
            {
                if (entry.Entity is IEntity entity)
                {
                    entity.IsDeleted = true;
                    entry.State = EntityState.Modified;
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
