using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class FakeDbContext : DbContext
    {
        private readonly FakeDatabaseFacade _fakeDatabaseFacade;

        public FakeDbContext()
        {
            _fakeDatabaseFacade = new FakeDatabaseFacade(this);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            base.OnConfiguring(optionsBuilder);
        }

        public override DatabaseFacade Database => _fakeDatabaseFacade;
    }

    public class FakeDatabaseFacade : DatabaseFacade
    {
        private IDbContextTransaction _currentTransaction;

        public static int TransactionCount { get; set; }
        public static int CommitCount { get; set; }
        public static int RollbackCount { get; set; }

        public FakeDatabaseFacade(DbContext context) : base(context)
        {
            TransactionCount = 0;
            CommitCount = 0;
            RollbackCount = 0;
        }

        public override IDbContextTransaction CurrentTransaction => _currentTransaction;

        public override IDbContextTransaction BeginTransaction()
        {
            _currentTransaction = new FakeTransaction(this);
            TransactionCount++;
            return _currentTransaction;
        }

        public override void CommitTransaction()
        {
            CommitCount++;
            _currentTransaction = null;
        }

        public override void RollbackTransaction()
        {
            RollbackCount++;
            _currentTransaction = null;
        }
    }
}
