using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using System.Reflection;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Configuration;

namespace Turner.Infrastructure.Crud.Tests
{
    public class BaseUnitTest
    {
        protected Container Container { get; set; }

        protected IMediator Mediator => Container.GetInstance<IMediator>();
        
        [SetUp]
        public void SetUp()
        {
            var assemblies = new[] { GetType().GetTypeInfo().Assembly };

            Container = new Container();
            Container.RegisterSingleton<DbContext>(() => new FakeDbContext());
            Container.ConfigureMediator(assemblies);
            Container.ConfigureCrud(assemblies);

            Mapper.Reset();
            Mapper.Initialize(config =>
            {
                config.AddProfiles(assemblies);
            });
        }
    }
}