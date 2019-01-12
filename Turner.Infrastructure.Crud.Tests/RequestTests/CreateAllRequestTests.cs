using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class CreateAllRequestTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_WithoutResponse_CreatesUsers()
        {
            var request = new CreateUsersWithoutResponseRequest
            {
                Users = new[]
                {
                    new UserDto { Name = "TestUser1" },
                    new UserDto { Name = "TestUser2" }
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            
            var users = await Context.Set<User>().ToListAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
            Assert.AreEqual("TestUser1", users[0].Name);
            Assert.AreEqual("/User[0]", users[0].PreMessage);
            Assert.AreEqual("PostCreate/Entity/User[TestUser1]", users[0].PostMessage);
            Assert.AreEqual("TestUser2", users[1].Name);
            Assert.AreEqual("/User[1]", users[1].PreMessage);
            Assert.AreEqual("PostCreate/Entity/User[TestUser2]", users[1].PostMessage);
        }
        
        [Test]
        public async Task Handle_WithResponse_CreatesUsersAndReturnsDtos()
        {
            var request = new CreateUsersWithResponseRequest
            {
                Users = new[]
                {
                    new UserDto { Name = "TestUser1" },
                    new UserDto { Name = "TestUser2" }
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);

            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("TestUser1", response.Data.Items[0].Name);
            Assert.AreEqual("/User[0]", response.Data.Items[0].PreMessage);
            Assert.AreEqual("PostCreate/Entity/User[TestUser1]", response.Data.Items[0].PostMessage);
            Assert.AreEqual("TestUser2", response.Data.Items[1].Name);
            Assert.AreEqual("/User[1]", response.Data.Items[1].PreMessage);
            Assert.AreEqual("PostCreate/Entity/User[TestUser2]", response.Data.Items[1].PostMessage);
        }

        [Test]
        public async Task Handle_DefaultWithoutResponse_CreatesUsers()
        {
            var request = new CreateAllRequest<User, UserDto>(new List<UserDto>
            {
                new UserDto { Name = "TestUser1" },
                new UserDto { Name = "TestUser2" }
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);

            var users = await Context.Set<User>().ToListAsync();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count);
            Assert.AreEqual("TestUser1", users[0].Name);
            Assert.AreEqual("PostCreate/Entity", users[0].PostMessage);
            Assert.AreEqual("TestUser2", users[1].Name);
            Assert.AreEqual("PostCreate/Entity", users[1].PostMessage);
        }

        [Test]
        public async Task Handle_DefaultWithResponse_CreatesUsersAndReturnsDtos()
        {
            var request = new CreateAllRequest<User, UserDto, UserGetDto>(new List<UserDto>
            {
                new UserDto { Name = "TestUser1" },
                new UserDto { Name = "TestUser2" }
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);

            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("TestUser1", response.Data.Items[0].Name);
            Assert.AreEqual("PostCreate/Entity", response.Data.Items[0].PostMessage);
            Assert.AreEqual("TestUser2", response.Data.Items[1].Name);
            Assert.AreEqual("PostCreate/Entity", response.Data.Items[1].PostMessage);
        }
    }

    public interface ICreateAllCommon : ICreateAllRequest
    {
        UserDto[] Users { get; set; }
    }

    [DoNotValidate]
    public class CreateUsersWithResponseRequest : ICreateAllCommon, ICreateAllRequest<User, UserGetDto>
    {
        public UserDto[] Users { get; set; }
    }

    [DoNotValidate]
    public class CreateUsersWithoutResponseRequest : ICreateAllCommon, ICreateAllRequest<User>
    {
        public UserDto[] Users { get; set; }
    }
    
    public class CreateUsersRequestProfile : CrudBulkRequestProfile<ICreateAllCommon, UserDto>
    {
        public CreateUsersRequestProfile()
        {
            ForEntity<User>()
                .WithData(request => request.Users)
                .CreateWith(user => Mapper.Map<User>(user))
                .BeforeCreating(request =>
                {
                    for (int i = 0; i < request.Users.Length; ++i)
                        request.Users[i].PreMessage += $"/User[{i}]";
                })
                .AfterCreating(user => user.PostMessage += $"/User[{user.Name}]");
        }
    }
}