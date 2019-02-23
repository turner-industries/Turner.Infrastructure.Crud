using System;
using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class HookConfig<THookFactory, THook>
    {
        private readonly Func<THookFactory, THook> _hookFactoryCreateFunc;

        private List<THookFactory> _hookFactories = new List<THookFactory>();

        protected HookConfig(Func<THookFactory, THook> hookFactoryCreateFunc)
        {
            _hookFactoryCreateFunc = hookFactoryCreateFunc;
        }

        internal void SetHooks(List<THookFactory> hookFactories)
        {
            _hookFactories = hookFactories;
        }

        internal void AddHooks(IEnumerable<THookFactory> hookFactories)
        {
            _hookFactories.AddRange(hookFactories);
        }

        public List<THook> GetHooks() => _hookFactories.Select(_hookFactoryCreateFunc).ToList();
    }

    public class RequestHookConfig
        : HookConfig<IRequestHookFactory, IBoxedRequestHook>
    {
        public RequestHookConfig() : base(factory => factory.Create())
        {
        }
    }

    public class EntityHookConfig
        : HookConfig<IEntityHookFactory, IBoxedEntityHook>
    {
        public EntityHookConfig() : base(factory => factory.Create())
        {
        }
    }

    public class ItemHookConfig
        : HookConfig<IItemHookFactory, IBoxedItemHook>
    {
        public ItemHookConfig() : base(factory => factory.Create())
        {
        }
    }

    public class ResultHookConfig
        : HookConfig<IResultHookFactory, IBoxedResultHook>
    {
        public ResultHookConfig() : base(factory => factory.Create())
        {
        }
    }
}
