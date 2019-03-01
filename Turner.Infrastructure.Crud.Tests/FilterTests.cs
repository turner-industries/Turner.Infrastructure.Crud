using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class FilterTests : BaseUnitTest
    {
        public static DateTime Date = new DateTime(2020, 2, 2, 12, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public async Task SetUp()
        {
            Context.AddRange(
                new Site { Name = "BADSite1", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(-4) },
                new Site { Name = "BADSite2", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(-2) },
                new Site { Name = "Site3BAD", Guid = Guid.NewGuid(), CreatedDate = Date },
                new Site { Name = "SiBDAte4BAD", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(2) },
                new Site { Name = "SiBDAte5", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(4) }
            );

            await Context.SaveChangesAsync();
        }

        [Test]
        public async Task Handle_IncludeBeforeConstantDate_FiltersEntities()
        {
            var request = new IncludeBeforeConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeBeforeRequestDate_FiltersEntities()
        {
            var request = new IncludeBeforeDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeBeforeConstantDate_FiltersEntities()
        {
            var request = new ExcludeBeforeConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeBeforeRequestDate_FiltersEntities()
        {
            var request = new ExcludeBeforeDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date, response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeAfterConstantDate_FiltersEntities()
        {
            var request = new IncludeAfterConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date, response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeAfterRequestDate_FiltersEntities()
        {
            var request = new IncludeAfterDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeAfterConstantDate_FiltersEntities()
        {
            var request = new ExcludeAfterConstantDateRequest();

            var response = await Mediator.HandleAsync(request);
            
            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeAfterRequestDate_FiltersEntities()
        {
            var request = new ExcludeAfterDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinConstantAndConstantDate_FiltersEntities()
        {
            var request = new IncludeWithinConstantAndConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinRequestAndConstantDate_FiltersEntities()
        {
            var request = new IncludeWithinRequestAndConstantDateRequest
            {
                MinDate = Date.AddDays(-3)
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinConstantAndRequestDate_FiltersEntities()
        {
            var request = new IncludeWithinConstantAndRequestDateRequest
            {
                MaxDate = Date.AddDays(3)
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinRequestAndRequestDate_FiltersEntities()
        {
            var request = new IncludeWithinRequestAndRequestDateRequest
            {
                MinDate = Date.AddDays(-3),
                MaxDate = Date.AddDays(3)
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeWithinConstantAndConstantDate_FiltersEntities()
        {
            var request = new ExcludeWithinConstantAndConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeWithinRequestAndConstantDate_FiltersEntities()
        {
            var request = new ExcludeWithinRequestAndConstantDateRequest
            {
                MinDate = Date.AddDays(-3)
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeWithinConstantAndRequestDate_FiltersEntities()
        {
            var request = new ExcludeWithinConstantAndRequestDateRequest
            {
                MaxDate = Date.AddDays(3)
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeWithinRequestAndRequestDate_FiltersEntities()
        {
            var request = new ExcludeWithinRequestAndRequestDateRequest
            {
                MinDate = Date.AddDays(-3),
                MaxDate = Date.AddDays(3)
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinExclusiveMinDate_FiltersEntities()
        {
            var request = new IncludeWithinExclusiveMinDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date, response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinExclusiveMaxDate_FiltersEntities()
        {
            var request = new IncludeWithinExclusiveMaxDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinExclusiveBothDate_FiltersEntities()
        {
            var request = new IncludeWithinExclusiveBothDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(1, response.Data.Items.Count);
            Assert.AreEqual(Date, response.Data.Items[0].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeStartsWithConstantString_FiltersEntities()
        {
            var request = new IncludeStartsWithConstantStringRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_IncludeStartsWithString_FiltersEntities()
        {
            var request = new IncludeStartsWithStringRequest { Name = "BAD" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_ExcludeStartsWithConstantString_FiltersEntities()
        {
            var request = new ExcludeStartsWithConstantStringRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual("Site3BAD", response.Data.Items[0].Name);
            Assert.AreEqual("SiBDAte4BAD", response.Data.Items[1].Name);
            Assert.AreEqual("SiBDAte5", response.Data.Items[2].Name);
        }

        [Test]
        public async Task Handle_ExcludeStartsWithString_FiltersEntities()
        {
            var request = new ExcludeStartsWithStringRequest { Name = "BAD" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual("Site3BAD", response.Data.Items[0].Name);
            Assert.AreEqual("SiBDAte4BAD", response.Data.Items[1].Name);
            Assert.AreEqual("SiBDAte5", response.Data.Items[2].Name);
        }

        [Test]
        public async Task Handle_IncludeEndsWithConstantString_FiltersEntities()
        {
            var request = new IncludeEndsWithConstantStringRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("Site3BAD", response.Data.Items[0].Name);
            Assert.AreEqual("SiBDAte4BAD", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_IncludeEndsWithString_FiltersEntities()
        {
            var request = new IncludeEndsWithStringRequest { Name = "BAD" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("Site3BAD", response.Data.Items[0].Name);
            Assert.AreEqual("SiBDAte4BAD", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_ExcludeEndsWithConstantString_FiltersEntities()
        {
            var request = new ExcludeEndsWithConstantStringRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
            Assert.AreEqual("SiBDAte5", response.Data.Items[2].Name);
        }

        [Test]
        public async Task Handle_ExcludeEndsWithString_FiltersEntities()
        {
            var request = new ExcludeEndsWithStringRequest { Name = "BAD" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
            Assert.AreEqual("SiBDAte5", response.Data.Items[2].Name);
        }

        [Test]
        public async Task Handle_IncludeContainsConstantString_FiltersEntities()
        {
            var request = new IncludeContainsConstantStringRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("SiBDAte4BAD", response.Data.Items[0].Name);
            Assert.AreEqual("SiBDAte5", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_IncludeContainsString_FiltersEntities()
        {
            var request = new IncludeContainsStringRequest { Name = "BDA" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("SiBDAte4BAD", response.Data.Items[0].Name);
            Assert.AreEqual("SiBDAte5", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_ExcludeContainsConstantString_FiltersEntities()
        {
            var request = new ExcludeContainsConstantStringRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
            Assert.AreEqual("Site3BAD", response.Data.Items[2].Name);
        }

        [Test]
        public async Task Handle_ExcludeContainsString_FiltersEntities()
        {
            var request = new ExcludeContainsStringRequest { Name = "BDA" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
            Assert.AreEqual("Site3BAD", response.Data.Items[2].Name);
        }

        [Test]
        public async Task Handle_ExcludeWithFalsePredicate_DoesNotFilterEntities()
        {
            var request = new ExcludeWithPredicateRequest { Name = string.Empty };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(5, response.Data.Items.Count);
        }

        [Test]
        public async Task Handle_ExcludeWithTruePredicate_FiltersEntities()
        {
            var request = new ExcludeWithPredicateRequest { Name = "BAD" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(1, response.Data.Items.Count);
        }
    }

    public class IncludeBeforeConstantDateRequest 
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeBeforeConstantDateProfile
        : CrudRequestProfile<IncludeBeforeConstantDateRequest>
    {
        public IncludeBeforeConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Before(FilterTests.Date.AddDays(1).Date));
        }
    }

    public class IncludeBeforeDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class IncludeBeforeDateProfile
        : CrudRequestProfile<IncludeBeforeDateRequest>
    {
        public IncludeBeforeDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder => 
                    builder.Include(e => e.CreatedDate).Before(r => r.Date));
        }
    }

    public class ExcludeBeforeConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class ExcludeBeforeConstantDateProfile
        : CrudRequestProfile<ExcludeBeforeConstantDateRequest>
    {
        public ExcludeBeforeConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).Before(FilterTests.Date.AddDays(1).Date));
        }
    }

    public class ExcludeBeforeDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class ExcludeBeforeDateProfile
        : CrudRequestProfile<ExcludeBeforeDateRequest>
    {
        public ExcludeBeforeDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).Before(r => r.Date));
        }
    }

    public class IncludeAfterConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeAfterConstantDateProfile
        : CrudRequestProfile<IncludeAfterConstantDateRequest>
    {
        public IncludeAfterConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).After(FilterTests.Date.AddDays(-1).Date));
        }
    }

    public class IncludeAfterDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class IncludeAfterDateProfile
        : CrudRequestProfile<IncludeAfterDateRequest>
    {
        public IncludeAfterDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).After(r => r.Date));
        }
    }

    public class ExcludeAfterConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class ExcludeAfterConstantDateProfile
        : CrudRequestProfile<ExcludeAfterConstantDateRequest>
    {
        public ExcludeAfterConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).After(FilterTests.Date.AddDays(-1).Date));
        }
    }

    public class ExcludeAfterDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class ExcludeAfterDateProfile
        : CrudRequestProfile<ExcludeAfterDateRequest>
    {
        public ExcludeAfterDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).After(r => r.Date));
        }
    }

    public class IncludeWithinConstantAndConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
    }

    public class IncludeWithinConstantAndConstantDateProfile
        : CrudRequestProfile<IncludeWithinConstantAndConstantDateRequest>
    {
        public IncludeWithinConstantAndConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Within(FilterTests.Date.AddDays(-3), FilterTests.Date.AddDays(3)));
        }
    }

    public class IncludeWithinRequestAndConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime MinDate { get; set; }
    }

    public class IncludeWithinRequestAndConstantDateProfile
        : CrudRequestProfile<IncludeWithinRequestAndConstantDateRequest>
    {
        public IncludeWithinRequestAndConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Within(r => r.MinDate, FilterTests.Date.AddDays(3)));
        }
    }

    public class IncludeWithinConstantAndRequestDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime MaxDate { get; set; }
    }

    public class IncludeWithinConstantAndRequestDateProfile
        : CrudRequestProfile<IncludeWithinConstantAndRequestDateRequest>
    {
        public IncludeWithinConstantAndRequestDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Within(FilterTests.Date.AddDays(-3), r => r.MaxDate));
        }
    }

    public class IncludeWithinRequestAndRequestDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }
    }

    public class IncludeWithinRequestAndRequestDateProfile
        : CrudRequestProfile<IncludeWithinRequestAndRequestDateRequest>
    {
        public IncludeWithinRequestAndRequestDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Within(r => r.MinDate, r => r.MaxDate));
        }
    }

    public class ExcludeWithinConstantAndConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
    }

    public class ExcludeWithinConstantAndConstantDateProfile
        : CrudRequestProfile<ExcludeWithinConstantAndConstantDateRequest>
    {
        public ExcludeWithinConstantAndConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).Within(FilterTests.Date.AddDays(-3), FilterTests.Date.AddDays(3)));
        }
    }

    public class ExcludeWithinRequestAndConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime MinDate { get; set; }
    }

    public class ExcludeWithinRequestAndConstantDateProfile
        : CrudRequestProfile<ExcludeWithinRequestAndConstantDateRequest>
    {
        public ExcludeWithinRequestAndConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).Within(r => r.MinDate, FilterTests.Date.AddDays(3)));
        }
    }

    public class ExcludeWithinConstantAndRequestDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime MaxDate { get; set; }
    }

    public class ExcludeWithinConstantAndRequestDateProfile
        : CrudRequestProfile<ExcludeWithinConstantAndRequestDateRequest>
    {
        public ExcludeWithinConstantAndRequestDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).Within(FilterTests.Date.AddDays(-3), r => r.MaxDate));
        }
    }

    public class ExcludeWithinRequestAndRequestDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }
    }

    public class ExcludeWithinRequestAndRequestDateProfile
        : CrudRequestProfile<ExcludeWithinRequestAndRequestDateRequest>
    {
        public ExcludeWithinRequestAndRequestDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.CreatedDate).Within(r => r.MinDate, r => r.MaxDate));
        }
    }

    public class IncludeWithinExclusiveMinDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
    }

    public class IncludeWithinExclusiveMinDateProfile
        : CrudRequestProfile<IncludeWithinExclusiveMinDateRequest>
    {
        public IncludeWithinExclusiveMinDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Within(
                        FilterTests.Date.AddDays(-2), 
                        FilterTests.Date.AddDays(2),
                        false, 
                        true));
        }
    }

    public class IncludeWithinExclusiveMaxDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
    }

    public class IncludeWithinExclusiveMaxDateProfile
        : CrudRequestProfile<IncludeWithinExclusiveMaxDateRequest>
    {
        public IncludeWithinExclusiveMaxDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Within(
                        FilterTests.Date.AddDays(-2),
                        FilterTests.Date.AddDays(2),
                        true,
                        false));
        }
    }

    public class IncludeWithinExclusiveBothDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
    }

    public class IncludeWithinExclusiveBothDateProfile
        : CrudRequestProfile<IncludeWithinExclusiveBothDateRequest>
    {
        public IncludeWithinExclusiveBothDateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.CreatedDate).Within(
                        FilterTests.Date.AddDays(-2),
                        FilterTests.Date.AddDays(2),
                        false,
                        false));
        }
    }

    public class IncludeStartsWithConstantStringRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeStartsWithConstantStringProfile
        : CrudRequestProfile<IncludeStartsWithConstantStringRequest>
    {
        public IncludeStartsWithConstantStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.Name).StartingWith("BAD"));
        }
    }

    public class IncludeStartsWithStringRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class IncludeStartsWithStringProfile
        : CrudRequestProfile<IncludeStartsWithStringRequest>
    {
        public IncludeStartsWithStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.Name).StartingWith(x => x.Name));
        }
    }

    public class ExcludeStartsWithConstantStringRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class ExcludeStartsWithConstantStringProfile
        : CrudRequestProfile<ExcludeStartsWithConstantStringRequest>
    {
        public ExcludeStartsWithConstantStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.Name).StartingWith("BAD"));
        }
    }

    public class ExcludeStartsWithStringRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class ExcludeStartsWithStringProfile
        : CrudRequestProfile<ExcludeStartsWithStringRequest>
    {
        public ExcludeStartsWithStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.Name).StartingWith(x => x.Name));
        }
    }

    public class IncludeEndsWithConstantStringRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeEndsWithConstantStringProfile
        : CrudRequestProfile<IncludeEndsWithConstantStringRequest>
    {
        public IncludeEndsWithConstantStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.Name).EndingWith("BAD"));
        }
    }

    public class IncludeEndsWithStringRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class IncludeEndsWithStringProfile
        : CrudRequestProfile<IncludeEndsWithStringRequest>
    {
        public IncludeEndsWithStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.Name).EndingWith(x => x.Name));
        }
    }

    public class ExcludeEndsWithConstantStringRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class ExcludeEndsWithConstantStringProfile
        : CrudRequestProfile<ExcludeEndsWithConstantStringRequest>
    {
        public ExcludeEndsWithConstantStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.Name).EndingWith("BAD"));
        }
    }

    public class ExcludeEndsWithStringRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class ExcludeEndsWithStringProfile
        : CrudRequestProfile<ExcludeEndsWithStringRequest>
    {
        public ExcludeEndsWithStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.Name).EndingWith(x => x.Name));
        }
    }

    public class IncludeContainsConstantStringRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeContainsConstantStringProfile
        : CrudRequestProfile<IncludeContainsConstantStringRequest>
    {
        public IncludeContainsConstantStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.Name).Containing("BDA"));
        }
    }

    public class IncludeContainsStringRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class IncludeContainsStringProfile
        : CrudRequestProfile<IncludeContainsStringRequest>
    {
        public IncludeContainsStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Include(e => e.Name).Containing(x => x.Name));
        }
    }

    public class ExcludeContainsConstantStringRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class ExcludeContainsConstantStringProfile
        : CrudRequestProfile<ExcludeContainsConstantStringRequest>
    {
        public ExcludeContainsConstantStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.Name).Containing("BDA"));
        }
    }

    public class ExcludeContainsStringRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class ExcludeContainsStringProfile
        : CrudRequestProfile<ExcludeContainsStringRequest>
    {
        public ExcludeContainsStringProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.Name).Containing(x => x.Name));
        }
    }

    public class ExcludeWithPredicateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class ExcludeWithPredicateProfile
        : CrudRequestProfile<ExcludeWithPredicateRequest>
    {
        public ExcludeWithPredicateProfile()
        {
            ForEntity<Site>()
                .FilterWith(builder =>
                    builder.Exclude(e => e.Name).Containing(x => x.Name).When(x => !string.IsNullOrWhiteSpace(x.Name)));
        }
    }
}