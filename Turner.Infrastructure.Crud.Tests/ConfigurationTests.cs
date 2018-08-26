using NUnit.Framework;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class ConfigurationTests : BaseUnitTest
    {
        private CrudProfileManager _profileManager;

        [SetUp]
        public void SetUp()
        {
            _profileManager = Container.GetInstance<CrudProfileManager>();
        }

        [Test]
        public void Test_EntityWithProfile_FindsEntityProfile()
        {
            var entityProfile = _profileManager.GetEntityProfileFor<Entity>();
            var userProfile = _profileManager.GetEntityProfileFor<User>();

            Assert.IsNotNull(entityProfile);
            Assert.IsNotNull(userProfile);
            Assert.AreNotEqual(entityProfile.GetType(), userProfile.GetType());
        }

        [Test]
        public void Test_EntityWithoutProfile_FindsBaseProfile()
        {
            var entityProfile = _profileManager.GetEntityProfileFor<Entity>();
            var siteProfile = _profileManager.GetEntityProfileFor<Site>();

            Assert.IsNotNull(entityProfile);
            Assert.IsNotNull(siteProfile);
            Assert.AreEqual(entityProfile.GetType(), siteProfile.GetType());
        }

        [Test]
        public void Test_EntityWithoutProfile_FindsDefaultProfile()
        {
            var defaultProfile = new DefaultCrudEntityProfile<NonEntity>();
            var profile = _profileManager.GetEntityProfileFor<NonEntity>();

            Assert.IsNotNull(defaultProfile);
            Assert.IsNotNull(profile);
            Assert.AreEqual(defaultProfile.GetType(), profile.GetType());
        }

        [Test]
        public void Test_DtoWithoutProfile_FindsBaseProfile()
        {
            var baseProfile = _profileManager.GetDtoProfileFor<UserDto>();
            var profile = _profileManager.GetDtoProfileFor<UserGetDto>();

            Assert.IsNotNull(baseProfile);
            Assert.IsNotNull(profile);
            Assert.AreEqual(baseProfile.GetType(), profile.GetType());
        }

        [Test]
        public void Test_DtoWithoutProfile_FindsDefaultProfile()
        {
            var defaultProfile = new DefaultCrudDtoProfile<NonEntityDto>();
            var profile = _profileManager.GetDtoProfileFor<NonEntityDto>();

            Assert.IsNotNull(defaultProfile);
            Assert.IsNotNull(profile);
            Assert.AreEqual(defaultProfile.GetType(), profile.GetType());
        }

        [Test]
        public void Test_RequestWithoutProfile_FindsDefaultProfile()
        {
            var defaultProfile = new DefaultCrudRequestProfile<CreateUserWithResponseRequest>();
            var createUserProfile = _profileManager.GetRequestProfileFor<CreateUserWithResponseRequest>();

            Assert.IsNotNull(defaultProfile);
            Assert.IsNotNull(createUserProfile);
            Assert.AreEqual(defaultProfile.GetType(), createUserProfile.GetType());
        }
        
        [Test]
        public void Test_FindAliases_HaveSameResult()
        {
            var entityProfile1 = _profileManager.GetEntityProfileFor(typeof(User));
            var entityProfile2 = _profileManager.GetEntityProfileFor<User>();

            var dtoProfile1 = _profileManager.GetDtoProfileFor(typeof(UserDto));
            var dtoProfile2 = _profileManager.GetDtoProfileFor<UserDto>();

            var requestProfile1 = _profileManager.GetRequestProfileFor(typeof(CreateUserWithoutResponseRequest));
            var requestProfile2 = _profileManager.GetRequestProfileFor<CreateUserWithoutResponseRequest>();

            Assert.IsNotNull(entityProfile1);
            Assert.AreEqual(entityProfile1.GetType(), entityProfile2.GetType());

            Assert.IsNotNull(dtoProfile1);
            Assert.AreEqual(dtoProfile1.GetType(), dtoProfile2.GetType());

            Assert.IsNotNull(requestProfile1);
            Assert.AreEqual(requestProfile1.GetType(), requestProfile2.GetType());
        }
    }
}