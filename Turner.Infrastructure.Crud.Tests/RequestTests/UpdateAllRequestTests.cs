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
    public class UpdateAllRequestTests : BaseUnitTest
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

            Context.AddRange(_users.Cast<object>());
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_UpdateAllUsersByIdRequest_UpdatesAllProvidedUsers()
        {
            var request = new UpdateAllUsersByIdRequest
            {
                Items = new List<UserGetDto>
                {
                    new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                    new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                    new UserGetDto { Id = 9999, Name = "Invalid Id" },
                    new UserGetDto { Id = _users[3].Id, Name = _users[3].Name }
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Data.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Data.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Data.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Data.Items[1].Name);
            Assert.AreEqual(_users[3].Id, response.Data.Items[2].Id);
            Assert.AreEqual("TestUser4", response.Data.Items[2].Name);
        }

        [Test]
        public async Task Handle_GenericUpdateAllByIdRequest_UpdatesAllProvidedUsers()
        {
            var request = new UpdateAllByIdRequest<User, UserGetDto, UserGetDto>(new List<UserGetDto>
            {
                new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                new UserGetDto { Id = 9999, Name = "Invalid Id" },
                new UserGetDto { Id = _users[3].Id, Name = _users[3].Name }
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Data.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Data.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Data.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Data.Items[1].Name);
            Assert.AreEqual(_users[3].Id, response.Data.Items[2].Id);
            Assert.AreEqual("TestUser4", response.Data.Items[2].Name);
        }
    }
    
    [DoNotValidate]
    public class UpdateAllUsersByIdRequest
        : IUpdateAllRequest<User, UserGetDto>
    {
        public List<UserGetDto> Items { get; set; }
    }

    public class UpdateAllUsersByIdProfile 
        : CrudBulkRequestProfile<UpdateAllUsersByIdRequest, UserGetDto>
    {
        public UpdateAllUsersByIdProfile()
        {
            ForEntity<User>()
                .WithData(request => request.Items)
                .WithKeys(item => item.Id, entity => entity.Id);
        }
    }
}