using System;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    public interface IBoxedItemHook
    {
        Task Run(object request, object item);
    }

    public interface IItemHookFactory
    {
        IBoxedItemHook Create();
    }

    public class FunctionItemHook
       : IBoxedItemHook
    {
        private readonly Func<object, object, Task> _hookFunc;

        public FunctionItemHook(Func<object, object, Task> hookFunc)
        {
            _hookFunc = hookFunc;
        }

        public Task Run(object request, object item) => _hookFunc(request, item);
    }
    
    public class FunctionItemHookFactory : IItemHookFactory
    {
        private readonly IBoxedItemHook _hook;

        private FunctionItemHookFactory(Func<object, object, Task> hook)
        {
            _hook = new FunctionItemHook(hook);
        }

        internal static FunctionItemHookFactory From<TRequest, TItem>(
            Func<TRequest, TItem, Task> hook)
        {
            return new FunctionItemHookFactory(
                (request, item) => hook((TRequest)request, (TItem)item));
        }

        internal static FunctionItemHookFactory From<TRequest, TItem>(
            Action<TRequest, TItem> hook)
        {
            return new FunctionItemHookFactory(
                (request, item) =>
                {
                    hook((TRequest)request, (TItem)item);
                    return Task.CompletedTask;
                });
        }

        public IBoxedItemHook Create() => _hook;
    }

    public class InstanceItemHookFactory : IItemHookFactory
    {
        private readonly object _instance;
        private IBoxedItemHook _adaptedInstance;

        private InstanceItemHookFactory(object instance, IBoxedItemHook adaptedInstance)
        {
            _instance = instance;
            _adaptedInstance = adaptedInstance;
        }

        internal static InstanceItemHookFactory From<TRequest, TItem>(
            IItemHook<TRequest, TItem> hook)
        {
            return new InstanceItemHookFactory(
                hook,
                new FunctionItemHook((request, item) => hook.Run((TRequest)request, (TItem)item)));
        }

        public IBoxedItemHook Create() => _adaptedInstance;
    }

    public class TypeItemHookFactory : IItemHookFactory
    {
        private static Func<Type, object> s_serviceFactory;

        private Func<IBoxedItemHook> _hookFactory;

        public TypeItemHookFactory(Func<IBoxedItemHook> hookFactory)
        {
            _hookFactory = hookFactory;
        }

        internal static void BindContainer(Func<Type, object> serviceFactory)
        {
            s_serviceFactory = serviceFactory;
        }

        internal static TypeItemHookFactory From<THook, TRequest, TItem>()
            where THook : IItemHook<TRequest, TItem>
        {
            return new TypeItemHookFactory(
                () =>
                {
                    var instance = (IItemHook<TRequest, TItem>)s_serviceFactory(typeof(THook));
                    return new FunctionItemHook((request, item) => instance.Run((TRequest)request, (TItem)item));
                });
        }

        public IBoxedItemHook Create() => _hookFactory();
    }
}
