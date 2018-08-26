using AutoMapper;
using System;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ICreateRequest<TEntity, TInput>
        : IRequest
        where TEntity : new()
    {

    }

    public interface ICreateRequest<TEntity, TInput, TOutput>
        : IRequest<TOutput>
        where TEntity : new()
    {

    }

    public abstract class CreateRequestHandlerBase<TRequest, TEntity, TInput>
        where TEntity : new()
    {
        protected Task<TEntity> CreateEntity(/*TInput data*/)
        {
            Console.WriteLine($"Creating a <{typeof(TEntity).Name}> " +
                            $"from a <{typeof(TInput).Name}> ");

            return Task.FromResult(new TEntity());
        }
    }

    public class CreateRequestHandler<TRequest, TEntity, TInput>
        : CreateRequestHandlerBase<TRequest, TEntity, TInput>,
          IRequestHandler<TRequest>
        where TRequest : ICreateRequest<TEntity, TInput>
        where TEntity : new()
    {
        public async Task<Response> HandleAsync(TRequest request)
        {
            // Need to be able to rip TInput from TRequest
            // should it be convertible or pulled out by an expression?  probably configurable

            var entity = await CreateEntity();

            return new Response();
        }
    }

    public class CreateRequestHandler<TRequest, TEntity, TInput, TOutput>
        : CreateRequestHandlerBase<TRequest, TEntity, TInput>,
          IRequestHandler<TRequest, TOutput>
        where TRequest : ICreateRequest<TEntity, TInput, TOutput>
        where TEntity : new()
    {
        private CrudProfileManager _profiles;

        public CreateRequestHandler(CrudProfileManager profiles)
        {
            _profiles = profiles;    
        }

        public async Task<Response<TOutput>> HandleAsync(TRequest request)
        {
            // Need to be able to rip TInput from TRequest
            // should it be convertible or pulled out by an expression?  probably configurable

            var entity = await CreateEntity();

            Console.WriteLine($"Mapping result to a <{typeof(TOutput)}>.");

            var result = Mapper.Map<TOutput>(entity);

            return new Response<TOutput> { Data = result };
        }
    }
}
