using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Reflection;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Crud.Validation;
using Turner.Infrastructure.Mediator.Configuration;

namespace Turner.Infrastructure.Crud.Tests
{
    [SetUpFixture]
    public class UnitTestSetUp
    {
        public static Container Container { get; private set; }

        [OneTimeSetUp]
        public static void UnitTestOneTimeSetUp()
        {
            var assemblies = new[] { typeof(UnitTestSetUp).Assembly };
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            ConfigureDatabase(container);
            ConfigureAutoMapper(container, assemblies);
            ConfigureFluentValidation(container, assemblies);

            container.ConfigureMediator(assemblies);

            var crudOptions = new CrudOptions
            {
                UseFluentValidation = true,
                UseEntityFramework = true
            };

            Crud.Configure(container, assemblies, crudOptions);
            
            Container = container;
        }

        public static void ConfigureDatabase(Container container)
        {
            container.Register<DbContext>(() =>
            {
                var options = new DbContextOptionsBuilder<FakeDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;

                return new FakeDbContext(options);
            }, 
            Lifestyle.Scoped);
        }

        public static void ConfigureAutoMapper(Container container, Assembly[] assemblies)
        {
            Mapper.Reset();
            Mapper.Initialize(config =>
            {
                config.AddProfiles(assemblies);
            });
        }

        public static void ConfigureFluentValidation(Container container, Assembly[] assemblies)
        {
            container.Register(typeof(FluentValidation.IValidator<>), assemblies);

            ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;
        }
    }
}
