using NUnit.Framework;
using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class ConfigurationTests : BaseUnitTest
    {
#if !DEPLOY
        private CrudConfigManager _profileManager;

        [SetUp]
        public void SetUp()
        {
            _profileManager = Container.GetInstance<CrudConfigManager>();
        }

        [Test]
        public void Test_RequestWithoutProfile_FindsDefaultConfig()
        {
            var defaultConfig = new CrudRequestConfig<CreateUserWithResponseRequest>();
            var createUserConfig = _profileManager.GetRequestConfigFor<CreateUserWithResponseRequest>();

            Assert.IsNotNull(defaultConfig);
            Assert.IsNotNull(createUserConfig);
            Assert.AreEqual(defaultConfig.GetType(), createUserConfig.GetType());
        }

        [Test]
        public void Test_FindAliases_HaveSameResult()
        {
            var requestConfig1 = _profileManager.GetRequestConfigFor(typeof(CreateUserWithoutResponseRequest));
            var requestConfig2 = _profileManager.GetRequestConfigFor<CreateUserWithoutResponseRequest>();

            Assert.IsNotNull(requestConfig1);
            Assert.AreEqual(requestConfig1.GetType(), requestConfig2.GetType());
        }
#endif
    }
}