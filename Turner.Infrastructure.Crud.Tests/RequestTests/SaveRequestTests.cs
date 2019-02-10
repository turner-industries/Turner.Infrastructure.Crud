using AutoMapper;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class SaveRequestTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_SaveWithoutResponse_CreatesUser()
        {
            var request = new SaveUserWithoutResponseRequest
            {
                User = new UserDto { Name = "TestUser" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("TestUser", user.Name);
        }

        [Test]
        public async Task Handle_SaveWithResponse_CreatesUser()
        {
            var request = new SaveUserWithResponseRequest
            {
                Name = "TestUser"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("TestUser", response.Data.Name);
        }

        [Test]
        public async Task Handle_SaveExistingWithoutResponse_UpdatesUser()
        {
            var existing = new User { Name = "TestUser" };
            await Context.AddAsync(existing);
            await Context.SaveChangesAsync();

            var request = new SaveUserWithoutResponseRequest
            { 
                Id = existing.Id,
                User = new UserDto { Name = "NewUser" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("NewUser", user.Name);
        }

        [Test]
        public async Task Handle_SaveExistingWithResponse_UpdatesUser()
        {
            var existing = new User { Name = "TestUser" };
            await Context.AddAsync(existing);
            await Context.SaveChangesAsync();

            var request = new SaveUserWithResponseRequest
            {
                Name = existing.Name
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("TestUser", response.Data.Name);
        }

        [Test]
        public async Task Handle_DefaultSaveWithoutResponseRequest_CreatesUser()
        {
            var request = new SaveRequest<User, UserGetDto>(new UserGetDto
            {
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            var user = Context.Set<User>().First();
            Assert.IsNotNull(user);
            Assert.AreNotEqual(0, user.Id);
            Assert.AreEqual("NewUser", request.Data.Name);
        }

        [Test]
        public async Task Handle_DefaultSaveWithResponseRequest_CreatesUser()
        {
            var request = new SaveRequest<User, UserGetDto, UserGetDto>(new UserGetDto
            {
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Data);
            Assert.AreNotEqual(0, response.Data.Id);
            Assert.AreEqual("NewUser", response.Data.Name);
        }
        
        [Test]
        public async Task Handle_SaveByIdRequest_UpdatesUser()
        {
            var existing = new User { Name = "TestUser" };
            await Context.AddAsync(existing);
            await Context.SaveChangesAsync();

            var request = new SaveByIdRequest<User, UserDto, UserGetDto>(
                existing.Id,
                new UserDto { Name = "NewUser" });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.AreEqual(existing.Id, response.Data.Id);
            Assert.AreEqual("NewUser", response.Data.Name);
            Assert.AreEqual("NewUser", Context.Set<User>().First().Name);
        }
    }
    
    public class SaveUserWithResponseRequest : UserDto, ISaveRequest<User, UserGetDto>
    { }
    
    public class SaveUserWithoutResponseRequest : ISaveRequest<User>
    {
        public int Id { get; set; }

        public UserDto User { get; set; }
    }
    
    public class SaveUserWithoutResponseProfile 
        : CrudRequestProfile<SaveUserWithoutResponseRequest>
    {
        public SaveUserWithoutResponseProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single(r => r.Id, e => e.Id))
                .CreateEntityWith(request => Mapper.Map<User>(request.User))
                .UpdateEntityWith((request, entity) => Mapper.Map(request.User, entity));
        }
    }

    public class SaveUserWithResponseProfile 
        : CrudRequestProfile<SaveUserWithResponseRequest>
    {
        public SaveUserWithResponseProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single("Name"));
        }
    }

    public class DefaultSaveWithoutResponseRequestProfile
        : CrudRequestProfile<SaveRequest<User, UserGetDto>>
    {
        public DefaultSaveWithoutResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single(r => e => r.Data.Id == e.Id));
        }
    }

    public class DefaultSaveWithResponseRequestProfile
        : CrudRequestProfile<SaveRequest<User, UserGetDto, UserGetDto>>
    {
        public DefaultSaveWithResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single(request => entity => request.Data.Id == entity.Id));
        }
    }
}