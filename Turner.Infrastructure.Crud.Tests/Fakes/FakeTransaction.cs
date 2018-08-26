using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class FakeTransaction : InMemoryTransaction
    {
        private readonly FakeDatabaseFacade _database;

        public FakeTransaction(FakeDatabaseFacade database)
        {
            _database = database;
        }

        public override void Commit()
        {
            _database.CommitTransaction();
        }

        public override void Rollback()
        {
            _database.RollbackTransaction();
        }
    }
}
