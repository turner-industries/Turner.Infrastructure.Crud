using AutoMapper;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class UpdateRequestTests : BaseUnitTest
    {
        User _user;

        [SetUp]
        public void SetUp()
        {
            _user = new User { Name = "TestUser" };

            Context.Add(_user);
            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_UpdateUserByIdRequest_UpdatesUser()
        {
            var request = new UpdateUserByIdRequest
            {
                Id = _user.Id,
                Name = "NewName"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual("NewName", response.Data.Name);
            Assert.AreEqual("PreUpdate/Update", response.Data.PreMessage);
            Assert.AreEqual("PostMessage/Update", response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_UpdateUserByNameRequest_UpdatesUser()
        {
            var request = new UpdateUserByNameRequest
            {
                Name = _user.Name,
                Data = new UserDto { Name = "NewName" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().FirstOrDefault();
            Assert.NotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.AreEqual("NewName", user.Name);
            Assert.AreEqual(_user.PreMessage, user.PreMessage);
            Assert.AreEqual(_user.PostMessage, user.PostMessage);
        }

        [Test]
        public async Task Handle_InvalidUpdateUserByNameRequest_ThrowsException()
        {
            FailedToFindException error = null;

            var request = new UpdateUserByNameRequest { Name = "NonUser", Data = new UserDto() };

            try
            {
                await Mediator.HandleAsync(request);
            }
            catch (FailedToFindException e)
            {
                error = e;
            }

            Assert.IsNotNull(error);
        }

        [Test]
        public async Task Handle_InvalidUpdateUserByIdRequest_ReturnsNull()
        {
            var request = new UpdateUserByIdRequest { Id = 100, Name = string.Empty };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task Handle_DefaultUpdateWithoutResponseRequest_UpdatesUser()
        {
            var request = new UpdateRequest<User, UserGetDto>(new UserGetDto
            {
                Id = _user.Id,
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().First();
            Assert.IsNotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.AreEqual(_user.Name, user.Name);
            Assert.AreEqual(_user.PreMessage, user.PreMessage);
            Assert.AreEqual(_user.PostMessage, user.PostMessage);
        }

        [Test]
        public async Task Handle_DefaultUpdateWithResponseRequest_UpdatesUser()
        {
            var request = new UpdateRequest<User, UserGetDto, UserGetDto>(new UserGetDto
            {
                Id = _user.Id,
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }
    }
    
    [DoNotValidate]
    public class UpdateUserByIdRequest 
        : UserDto, IUpdateRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }

    [DoNotValidate]
    public class UpdateUserByNameRequest
        : IUpdateRequest<User>
    {
        public string Name { get; set; }
        public UserDto Data { get; set; }
    }

    public class UpdateRequestProfile<TEntity, TOut> 
        : CrudRequestProfile<IUpdateRequest<TEntity, TOut>>
        where TEntity : class, IEntity
    {
        public UpdateRequestProfile()
        {
            ForEntity<IEntity>()
                .AfterUpdating(entity => entity.PostMessage = "PostMessage");
        }
    }

    public class UpdateUserWithoutResponseRequestProfile 
        : CrudRequestProfile<UpdateRequest<User, UserGetDto>>
    {
        public UpdateUserWithoutResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectForUpdateWith(request => entity => request.Data.Id == entity.Id);
        }
    }

    public class UpdateUserWithResponseRequestProfile 
        : CrudRequestProfile<UpdateRequest<User, UserGetDto, UserGetDto>>
    {
        public UpdateUserWithResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectForUpdateWith(request => entity => request.Data.Id == entity.Id);
        }
    }

    public class UpdateUserByIdProfile : CrudRequestProfile<UpdateUserByIdRequest>
    {
        public UpdateUserByIdProfile()
        {
            ForEntity<User>()
                .SelectForUpdateWith(request => entity => entity.Id == request.Id)
                .BeforeUpdating(request => request.PreMessage += "/Update")
                .AfterUpdating(entity => entity.PostMessage += "/Update");

            ConfigureErrors(config => config.FailedToFindInUpdateIsError = false);
        }
    }

    public class UpdateUserByNameProfile : CrudRequestProfile<UpdateUserByNameRequest>
    {
        public UpdateUserByNameProfile()
        {
            ForEntity<User>()
                .SelectForAnyWith(request => entity =>
                    string.Equals(entity.Name, request.Name, StringComparison.InvariantCultureIgnoreCase))
                .UpdateWith((request, entity) => 
                    Task.FromResult(Mapper.Map(request.Data, entity)));

            ConfigureErrors(config => config.FailedToFindInUpdateIsError = true);
        }
    }
}