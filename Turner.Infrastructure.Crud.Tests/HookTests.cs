using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class HookTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_TestHooks_RunsAllHooksInCorrectOrder()
        {
            var request = new TestHooksRequest
            {
                Items = new List<HookDto>
                {
                    new HookDto(),
                    new HookDto()
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(2, Context.Set<HookEntity>().Count());

            var entity = Context.Set<HookEntity>().First();
            Assert.AreEqual("r1/r2/r3/r4", entity.RequestHookMessage);
            Assert.AreEqual("e1/e2/e3/e4", entity.EntityHookMessage);
            Assert.AreEqual("i1/i2/i3/i4", entity.ItemHookMessage);
            Assert.AreEqual("t1/t2/t3/t4", response.Data.Items[0]);
        }
    }
    
    public class HookDto
    {
        public int Id { get; set; }

        public string RequestHookMessage { get; set; }

        public string EntityHookMessage { get; set; }

        public string ItemHookMessage { get; set; }
    }
    
    public interface ITestHooksRequest : ICreateAllRequest<HookEntity, string>
    {
        List<HookDto> Items { get; set; }
    }

    [DoNotValidate]
    public class TestHooksRequest : ITestHooksRequest
    {
        public List<HookDto> Items { get; set; } = new List<HookDto>();
    }

    public class TestInstanceRequestHook : IRequestHook<TestHooksRequest>
    {
        public Task Run(TestHooksRequest request)
        {
            request.Items.ForEach(i => i.RequestHookMessage += "r3/");
            return Task.CompletedTask;
        }
    }

    public class TestInstanceEntityHook : IEntityHook<TestHooksRequest, HookEntity>
    {
        public Task Run(TestHooksRequest request, HookEntity entity)
        {
            entity.EntityHookMessage += "e3/";
            return Task.CompletedTask;
        }
    }

    public class TestInstanceItemHook : IItemHook<TestHooksRequest, HookDto>
    {
        public Task<HookDto> Run(TestHooksRequest request, HookDto item)
        {
            item.ItemHookMessage += "i3/";
            return Task.FromResult(item);
        }
    }

    public class TestInstanceResultHook : IResultHook<TestHooksRequest, string>
    {
        public Task<string> Run(TestHooksRequest request, string result)
        {
            return Task.FromResult(result + "t3/");
        }
    }

    public class TestTypeRequestHook : IRequestHook<TestHooksRequest>
    {
        public TestTypeRequestHook(DbContext context)
        {
            if (context == null)
                throw new System.Exception("Injection Failed");
        }

        public Task Run(TestHooksRequest request)
        {
            request.Items.ForEach(i => i.RequestHookMessage += "r4");
            return Task.CompletedTask;
        }
    }

    public class TestTypeEntityHook : IEntityHook<TestHooksRequest, HookEntity>
    {
        public TestTypeEntityHook(DbContext context)
        {
            if (context == null)
                throw new System.Exception("Injection Failed");
        }

        public Task Run(TestHooksRequest request, HookEntity entity)
        {
            entity.EntityHookMessage += "e4";
            return Task.CompletedTask;
        }
    }

    public class TestTypeItemHook : IItemHook<TestHooksRequest, HookDto>
    {
        public TestTypeItemHook(DbContext context)
        {
            if (context == null)
                throw new System.Exception("Injection Failed");
        }

        public Task<HookDto> Run(TestHooksRequest request, HookDto item)
        {
            item.ItemHookMessage += "i4";
            return Task.FromResult(item);
        }
    }

    public class TestTypeResultHook : IResultHook<TestHooksRequest, string>
    {
        public TestTypeResultHook(DbContext context)
        {
            if (context == null)
                throw new System.Exception("Injection Failed");
        }

        public Task<string> Run(TestHooksRequest request, string result)
        {
            return Task.FromResult(result + "t4");
        }
    }

    public class TestHooksProfile
        : CrudBulkRequestProfile<ITestHooksRequest, HookDto>
    {
        public TestHooksProfile() : base(r => r.Items)
        {
            WithRequestHook(r => r.Items.ForEach(i => i.RequestHookMessage = "r1/"));
            WithRequestHook(r =>
            {
                r.Items.ForEach(i => i.RequestHookMessage += "r2/");
                return Task.CompletedTask;
            });

            WithResultHook<string>((r, t) => "t1/");
            WithResultHook<string>((r, t) => Task.FromResult(t + "t2/"));

            ForEntity<IHookEntity>()
                .WithEntityHook((r, e) => e.EntityHookMessage = "e1/")
                .WithEntityHook((r, e) =>
                {
                    e.EntityHookMessage += "e2/";
                    return Task.CompletedTask;
                })
                .WithItemHook((r, i) =>
                {
                    i.ItemHookMessage += "i1/";
                    return i;
                })
                .WithItemHook((r, i) =>
                {
                    i.ItemHookMessage += "i2/";
                    return Task.FromResult(i);
                });
        }
    }

    public class TestHooksRequestProfile
        : CrudBulkRequestProfile<TestHooksRequest, HookDto>
    {
        public TestHooksRequestProfile() : base(r => r.Items)
        {
            WithRequestHook(new TestInstanceRequestHook());
            WithRequestHook<TestTypeRequestHook>();

            WithResultHook(new TestInstanceResultHook());
            WithResultHook<TestTypeResultHook, string>();

            ForEntity<HookEntity>()
                .CreateResultWith(x => string.Empty)
                .WithEntityHook(new TestInstanceEntityHook())
                .WithEntityHook<TestTypeEntityHook>()
                .WithItemHook(new TestInstanceItemHook())
                .WithItemHook<TestTypeItemHook>();
        }
    }
}