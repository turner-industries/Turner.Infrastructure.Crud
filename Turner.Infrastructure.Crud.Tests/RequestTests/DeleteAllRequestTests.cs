using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class DeleteAllRequestTests : BaseUnitTest
    {
        User[] _users;

        [SetUp]
        public void SetUp()
        {
            _users = new[]
            {
                new User { Name = "TestUser1" },
                new User { Name = "TestUser2" },
                new User { Name = "TestUser3" },
                new User { Name = "TestUser4" }
            };

            Context.AddRange(_users);
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_DeleteAllUsersByIdRequest_DeletesAllFilteredUsers()
        {
            var request = new DeleteAllUsersByIdRequest
            {
                Ids = new List<int>
                {
                    _users[0].Id,
                    _users[2].Id
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.IsTrue(response.Data.Items[0].IsDeleted);
            Assert.AreEqual("PostDelete/Entity/Delete", response.Data.Items[0].PostMessage);
            Assert.IsTrue(response.Data.Items[1].IsDeleted);
            Assert.AreEqual("PostDelete/Entity/Delete", response.Data.Items[1].PostMessage);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser2").IsDeleted);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser4").IsDeleted);
        }

        [Test]
        public async Task Handle_DeleteAllByIdRequest_DeletesAllFilteredUsers()
        {
            var request = new DeleteAllByIdRequest<User, UserGetDto>
            (
                new List<int>
                {
                    _users[0].Id,
                    _users[2].Id
                }
            );

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.IsTrue(response.Data.Items[0].IsDeleted);
            Assert.IsTrue(response.Data.Items[1].IsDeleted);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser2").IsDeleted);
            Assert.IsFalse(Context.Set<User>().First(x => x.Name == "TestUser4").IsDeleted);
        }
    }

    [DoNotValidate]
    public class DeleteAllUsersByIdRequest
        : IDeleteAllRequest<User, UserGetDto>
    {
        public List<int> Ids { get; set; }
    }
    
    public class DeleteAllUsersByIdProfile 
        : CrudRequestProfile<DeleteAllUsersByIdRequest>
    {
        public DeleteAllUsersByIdProfile()
        {
            ForEntity<User>()
                .FilterWith(builder => builder.FilterOn(r => r.Ids, "Id"))
                .AfterDeleting(entity => entity.PostMessage += "/Delete");

            ConfigureErrors(config => config.FailedToFindInDeleteIsError = false);
        }
    }
}