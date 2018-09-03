using AutoMapper;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
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
            Assert.AreEqual("/Save", user.PostMessage);
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
            Assert.AreEqual("PostCreate/Entity/Save", response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_SaveExistingWithoutResponse_UpdatesUser()
        {
            var existing = new User { Name = "TestUser", PreMessage = "Foo" };
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
            Assert.AreEqual("PostUpdate/Entity/Save", user.PostMessage);
        }

        [Test]
        public async Task Handle_SaveExistingWithResponse_UpdatesUser()
        {
            var existing = new User { Name = "TestUser", PreMessage = "Foo" };
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
            Assert.AreEqual(null, response.Data.PreMessage);
            Assert.AreEqual(null, Context.Set<User>().First().PreMessage);
            Assert.AreEqual("/Save", response.Data.PostMessage);
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
            Assert.AreEqual("NewUser", user.Name);
            Assert.AreEqual("PostSave", request.Data.Name);
            Assert.AreEqual(null, user.PreMessage);
            Assert.AreEqual("PostCreate/Entity", user.PostMessage);
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
            Assert.AreEqual(null, response.Data.PreMessage);
            Assert.AreEqual("PostCreate/Entity", response.Data.PostMessage);
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

    [DoNotValidate]
    public class SaveUserWithResponseRequest : UserDto, ISaveRequest<User, UserGetDto>
    { }

    [DoNotValidate]
    public class SaveUserWithoutResponseRequest : ISaveRequest<User>
    {
        public int Id { get; set; }
        public UserDto User { get; set; }
    }
    
    public class SaveRequestProfile : CrudRequestProfile<ISaveRequest>
    {
        public SaveRequestProfile()
        {
            ForEntity<IEntity>()
                .AfterCreating(entity => entity.PostMessage = "PostCreate/Entity")
                .AfterUpdating(entity => entity.PostMessage = "PostUpdate/Entity");
        }
    }

    public class SaveUserWithoutResponseProfile : CrudRequestProfile<SaveUserWithoutResponseRequest>
    {
        public SaveUserWithoutResponseProfile()
        {
            ForEntity<User>()
                .SelectForUpdateWith(builder => builder.Build(r => r.Id, e => e.Id))
                .CreateWith(request => Mapper.Map<User>(request.User))
                .UpdateWith((request, entity) => Mapper.Map(request.User, entity))
                .AfterSaving(entity => entity.PostMessage += "/Save")
                .ConfigureOptions(options => options.SuppressCreateActionsInSave = true);
        }
    }

    public class SaveUserWithResponseProfile : CrudRequestProfile<SaveUserWithResponseRequest>
    {
        public SaveUserWithResponseProfile()
        {
            ForEntity<User>()
                .SelectForUpdateWith(builder => builder.Build("Name"))
                .AfterUpdating(entity => entity.PostMessage = "PostUpdate/Entity")
                .AfterSaving(entity => entity.PostMessage += "/Save")
                .ConfigureOptions(options => options.SuppressUpdateActionsInSave = true);
        }
    }

    public class DefaultSaveWithoutResponseRequestProfile
        : CrudRequestProfile<SaveRequest<User, UserGetDto>>
    {
        public DefaultSaveWithoutResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectForUpdateWith(builder => builder.Build(r => e => r.Data.Id == e.Id));

            AfterSaving(request => request.Data.Name = "PostSave");
        }
    }

    public class DefaultSaveWithResponseRequestProfile
        : CrudRequestProfile<SaveRequest<User, UserGetDto, UserGetDto>>
    {
        public DefaultSaveWithResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectForUpdateWith(builder => builder.Build(request => entity => request.Data.Id == entity.Id));
        }
    }
}