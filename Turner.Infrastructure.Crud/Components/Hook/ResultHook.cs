using System;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    public interface IBoxedResultHook
    {
        Type ResultType { get; }

        Task<object> Run(object request, object result, CancellationToken ct = default(CancellationToken));
    }

    public interface IResultHookFactory
    {
        IBoxedResultHook Create();
    }

    public class FunctionResultHook
       : IBoxedResultHook
    {
        private readonly Func<object, object, CancellationToken, Task<object>> _hookFunc;

        public Type ResultType { get; }

        public FunctionResultHook(Type resultType, Func<object, object, CancellationToken, Task<object>> hookFunc)
        {
            ResultType = resultType;
            _hookFunc = hookFunc;
        }

        public Task<object> Run(object request, object result, CancellationToken ct) 
            => _hookFunc(request, result, ct);
    }

    public class FunctionResultHookFactory : IResultHookFactory
    {
        private readonly IBoxedResultHook _hook;
        
        private FunctionResultHookFactory(Type resultType, Func<object, object, CancellationToken, Task<object>> hook)
        {
            _hook = new FunctionResultHook(resultType, hook);
        }

        internal static FunctionResultHookFactory From<TRequest, TResult>(
            Func<TRequest, TResult, CancellationToken, Task<TResult>> hook)
        {
            return new FunctionResultHookFactory(typeof(TResult),
                (request, result, ct) => hook((TRequest)request, (TResult)result, ct)
                    .ContinueWith(t => (object)t.Result));
        }

        internal static FunctionResultHookFactory From<TRequest, TResult>(
            Func<TRequest, TResult, TResult> hook)
        {
            return new FunctionResultHookFactory(
                typeof(TResult),
                (request, result, ct) => 
                {
                    if (ct.IsCancellationRequested)
                        return Task.FromCanceled<object>(ct);

                    return Task.FromResult((object)hook((TRequest)request, (TResult)result));
                });
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
                new FunctionResultHook(typeof(TResult), 
                    (request, result, ct) => hook
                        .Run((TRequest)request, (TResult)result, ct)
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
                    return new FunctionResultHook(typeof(TResult), (request, result, ct) 
                        => instance.Run((TRequest)request, (TResult)result, ct).ContinueWith(t => (object)t.Result));
                });
        }

        public IBoxedResultHook Create() => _hookFactory();
    }
}
