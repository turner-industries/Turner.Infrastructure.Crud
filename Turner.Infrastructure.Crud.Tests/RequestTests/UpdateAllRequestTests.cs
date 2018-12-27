using AutoMapper;
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

            Context.AddRange(_users);
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_UpdateAllUsersByIdRequest_UpdatesAllFilteredUsers()
        {
            var request = new UpdateAllUsersByIdRequest
            {
                Items = new List<UserGetDto>
                {
                    new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                    new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                    new UserGetDto { Id = 9999, Name = "Invalid Id" },
                    new UserGetDto { Id = _users[3].Id, Name = _users[3].Name },
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
    public class UpdateAllUsersByIdRequest
        : IUpdateAllRequest<User, UserGetDto>
    {
        public List<UserGetDto> Items { get; set; }
    }

    public class UpdateAllUsersByIdProfile : CrudRequestProfile<UpdateAllUsersByIdRequest>
    {
        public UpdateAllUsersByIdProfile()
        {
            // TODO: Should not have to select on x.Id when filtering (combine into single config)
            ForEntity<User>()
                .FilterWith(builder => builder.FilterOnCollection(r => r.Items.Select(x => x.Id), "Id"))
                .UpdateAllWith((request, users) =>
                {
                    // TODO: A builder that creates the join (and maybe the filter too)
                    return request.Items
                        .Join(users, x => x.Id, x => x.Id,
                            (item, user) => new { In = item, Out = user })
                        .ForEach(x => Mapper.Map(x.In, x.Out))
                        .Select(x => x.Out)
                        .ToArray();
                });

            ConfigureErrors(config => config.FailedToFindInDeleteIsError = false);
        }
    }
}