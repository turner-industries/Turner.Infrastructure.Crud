using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class SynchronizeRequestTests : BaseUnitTest
    {
        User[] _users;

        [SetUp]
        public void SetUp()
        {
            _users = new[]
            {
                new User { Name = "TestUser1" },
                new User { Name = "TestUser2" },
                new User { Name = "TestUser3Filtered" },
                new User { Name = "TestUser4" },
                new User { Name = "TestUser5Filtered" }
            };

            Context.AddRange(_users.Cast<object>());
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_SynchronizeUsersByIdRequest_SynchronizesAllProvidedUsers()
        {
            var request = new SynchronizeUsersByIdRequest
            {
                Items = new List<UserGetDto>
                {
                    new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                    new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                    new UserGetDto { Id = 9999, Name = "NewUser1" },
                    new UserGetDto { Id = 0, Name = "NewUser2" },
                    new UserGetDto { Name = "NewUser3" },
                    null,
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(5, response.Data.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Data.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Data.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Data.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Data.Items[1].Name);
            Assert.AreNotEqual(0, response.Data.Items[2].Id);
            Assert.AreEqual("NewUser1", response.Data.Items[2].Name);
            Assert.AreNotEqual(0, response.Data.Items[3].Id);
            Assert.AreEqual("NewUser2", response.Data.Items[3].Name);
            Assert.AreNotEqual(0, response.Data.Items[4].Id);
            Assert.AreEqual("NewUser3", response.Data.Items[4].Name);
            Assert.AreEqual(3, Context.Set<User>().Count(x => x.IsDeleted));
            Assert.AreEqual(5, Context.Set<User>().Count(x => !x.IsDeleted));
        }

        [Test]
        public async Task Handle_GenericSynchronizeByIdRequest_UpdatesAllProvidedUsers()
        {
            var request = new SynchronizeByIdRequest<User, UserGetDto, UserGetDto>(new List<UserGetDto>
            {
                new UserGetDto { Id = _users[0].Id, Name = string.Concat(_users[0].Name, "_New") },
                    new UserGetDto { Id = _users[1].Id, Name = string.Concat(_users[1].Name, "_New") },
                    new UserGetDto { Id = 9999, Name = "NewUser1" },
                    new UserGetDto { Id = 0, Name = "NewUser2" },
                    new UserGetDto { Name = "NewUser3" },
                    null,
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(5, response.Data.Items.Count);
            Assert.AreEqual(_users[0].Id, response.Data.Items[0].Id);
            Assert.AreEqual("TestUser1_New", response.Data.Items[0].Name);
            Assert.AreEqual(_users[1].Id, response.Data.Items[1].Id);
            Assert.AreEqual("TestUser2_New", response.Data.Items[1].Name);
            Assert.AreNotEqual(0, response.Data.Items[2].Id);
            Assert.AreEqual("NewUser1", response.Data.Items[2].Name);
            Assert.AreNotEqual(0, response.Data.Items[3].Id);
            Assert.AreEqual("NewUser2", response.Data.Items[3].Name);
            Assert.AreNotEqual(0, response.Data.Items[4].Id);
            Assert.AreEqual("NewUser3", response.Data.Items[4].Name);
            Assert.AreEqual(3, Context.Set<User>().Count(x => x.IsDeleted));
            Assert.AreEqual(5, Context.Set<User>().Count(x => !x.IsDeleted));
        }

        [Test]
        public async Task Handle_SynchronizeUserClaimsRequest_SynchronizesWithoutDeletingOtherUsersClaims()
        {
            var user1Claims = new[]
            {
                new UserClaim { UserId = _users[0].Id, Claim = "TestClaim1" },
                new UserClaim { UserId = _users[0].Id, Claim = "TestClaim2" }
            };

            Context.AddRange(user1Claims.Cast<object>());

            var user2Claims = new[]
            {
                new UserClaim { UserId = _users[1].Id, Claim = "TestClaim3" },
                new UserClaim { UserId = _users[1].Id, Claim = "TestClaim4" }
            };

            Context.AddRange(user2Claims.Cast<object>());
            Context.SaveChanges();

            var request = new SynchronizeUserClaimsRequest
            {
                UserId = _users[1].Id,
                Claims = new List<string>
                {
                    "TestClaim3",
                    "TestClaim5"
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("TestClaim3", response.Data.Items[0]);
            Assert.AreEqual("TestClaim5", response.Data.Items[1]);
            Assert.AreEqual(1, Context.Set<UserClaim>().Count(x => x.IsDeleted));
            Assert.AreEqual(4, Context.Set<UserClaim>().Count(x => !x.IsDeleted));
            Assert.AreEqual(2, Context.Set<UserClaim>().Count(x => x.UserId == _users[0].Id && !x.IsDeleted));
            Assert.AreEqual(1, Context.Set<UserClaim>().Count(x => x.UserId == _users[1].Id && x.IsDeleted));
            Assert.AreEqual(2, Context.Set<UserClaim>().Count(x => x.UserId == _users[1].Id && !x.IsDeleted));
        }
    }
    
    public class SynchronizeUsersByIdRequest
        : ISynchronizeRequest<User, UserGetDto>
    {
        public List<UserGetDto> Items { get; set; }
    }

    public class SynchronizeUsersByIdProfile
        : CrudBulkRequestProfile<SynchronizeUsersByIdRequest, UserGetDto>
    {
        public SynchronizeUsersByIdProfile() : base(request => request.Items)
        {
            ForEntity<User>().UseKeys("Id");
        }
    }

    public class SynchronizeUserClaimsRequest
        : ISynchronizeRequest<UserClaim, string>
    {
        public int UserId { get; set; }

        public List<string> Claims { get; set; }
    }

    public class NotDeletedFilter : IFilter<ICrudRequest, IEntity>
    {
        public IQueryable<IEntity> Filter(ICrudRequest request, IQueryable<IEntity> queryable)
        {
            return queryable.Where(x => !x.IsDeleted);
        }
    }

    public class SynchronizeUserClaimsProfile
        : CrudBulkRequestProfile<SynchronizeUserClaimsRequest, string>
    {
        public SynchronizeUserClaimsProfile() : base(request => request.Claims)
        {
            ForEntity<UserClaim>()
                .UseKeys(x => x, x => x.Claim)
                .FilterWith(new NotDeletedFilter())
                .FilterUsing((request, claim) => request.UserId == claim.UserId)
                .CreateResultWith(x => x.Claim)
                .UpdateEntityWith((claim, entity) => entity)
                .CreateEntityWith((request, claim) => new UserClaim
                {
                    UserId = request.UserId,
                    Claim = claim
                });
        }
    }
}