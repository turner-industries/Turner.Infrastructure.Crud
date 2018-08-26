using System;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ICreateRequest<TEntity, TInput>
        : IRequest
    {

    }

    public interface ICreateRequest<TEntity, TInput, TOutput>
        : IRequest<TOutput>
    {

    }

    public abstract class CreateRequestHandlerBase<TRequest, TEntity, TInput>
    {
        protected void CreateEntity(/*TInput data*/)
        {
            Console.WriteLine($"Creating a <{typeof(TEntity).Name}> " +
                            $"from a <{typeof(TInput).Name}> ");
        }
    }

    public class CreateRequestHandler<TRequest, TEntity, TInput>
        : CreateRequestHandlerBase<TRequest, TEntity, TInput>,
          IRequestHandler<TRequest>
        where TRequest : ICreateRequest<TEntity, TInput>
    {
        public Task<Response> HandleAsync(TRequest request)
        {
            // Need to be able to rip TInput from TRequest
            // should it be convertible or pulled out by an expression?  probably configurable

            CreateEntity();

            return Task.FromResult(new Response());
        }
    }

    public class CreateRequestHandler<TRequest, TEntity, TInput, TOutput>
        : CreateRequestHandlerBase<TRequest, TEntity, TInput>,
          IRequestHandler<TRequest, TOutput>
        where TRequest : ICreateRequest<TEntity, TInput, TOutput>
    {
        private CrudProfileManager _profiles;

        public CreateRequestHandler(CrudProfileManager profiles)
        {
            _profiles = profiles;    
        }

        public Task<Response<TOutput>> HandleAsync(TRequest request)
        {
            // Need to be able to rip TInput from TRequest
            // should it be convertible or pulled out by an expression?  probably configurable

            CreateEntity();

            Console.WriteLine($"Mapping result to a <{typeof(TOutput)}>.");

            return Task.FromResult(new Response<TOutput>());
        }
    }
}
