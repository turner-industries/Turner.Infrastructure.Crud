using System;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    public interface IBoxedResultHook
    {
        Task<object> Run(object request, object result);
    }

    public interface IResultHookFactory
    {
        IBoxedResultHook Create();
    }

    public class FunctionResultHook
       : IBoxedResultHook
    {
        private readonly Func<object, object, Task<object>> _hookFunc;

        public FunctionResultHook(Func<object, object, Task<object>> hookFunc)
        {
            _hookFunc = hookFunc;
        }

        public Task<object> Run(object request, object result) => _hookFunc(request, result);
    }

    public class FunctionResultHookFactory : IResultHookFactory
    {
        private readonly IBoxedResultHook _hook;

        private FunctionResultHookFactory(Func<object, object, Task<object>> hook)
        {
            _hook = new FunctionResultHook(hook);
        }

        internal static FunctionResultHookFactory From<TRequest, TResult>(
            Func<TRequest, TResult, Task<TResult>> hook)
        {
            return new FunctionResultHookFactory(
                (request, result) => hook((TRequest)request, (TResult)result)
                    .ContinueWith(t => (object)t.Result));
        }

        internal static FunctionResultHookFactory From<TRequest, TResult>(
            Func<TRequest, TResult, TResult> hook)
        {
            return new FunctionResultHookFactory(
                (request, result) => Task.FromResult((object)hook((TRequest)request, (TResult)result)));
        }

        public IBoxedResultHook Create() => _hook;
    }

    public class InstanceResultHookFactory : IResultHookFactory
    {
        private readonly object _instance;
        private IBoxedResultHook _adaptedInstance;

        private InstanceResultHookFactory(object instance, IBoxedResultHook adaptedInstance)
        {
            _instance = instance;
            _adaptedInstance = adaptedInstance;
        }

        internal static InstanceResultHookFactory From<TRequest, TResult>(
            IResultHook<TRequest, TResult> hook)
        {
            return new InstanceResultHookFactory(
                hook,
                new FunctionResultHook((request, result) => hook.Run((TRequest)request, (TResult)result)
                    .ContinueWith(t => (object)t.Result)));
        }

        public IBoxedResultHook Create() => _adaptedInstance;
    }

    public class TypeResultHookFactory : IResultHookFactory
    {
        private static Func<Type, object> s_serviceFactory;

        private Func<IBoxedResultHook> _hookFactory;

        public TypeResultHookFactory(Func<IBoxedResultHook> hookFactory)
        {
            _hookFactory = hookFactory;
        }

        internal static void BindContainer(Func<Type, object> serviceFactory)
        {
            s_serviceFactory = serviceFactory;
        }

        internal static TypeResultHookFactory From<THook, TRequest, TResult>()
            where THook : IResultHook<TRequest, TResult>
        {
            return new TypeResultHookFactory(
                () =>
                {
                    var instance = (IResultHook<TRequest, TResult>)s_serviceFactory(typeof(THook));
                    return new FunctionResultHook((request, result) 
                        => instance.Run((TRequest)request, (TResult)result).ContinueWith(t => (object)t.Result));
                });
        }

        public IBoxedResultHook Create() => _hookFactory();
    }
}
