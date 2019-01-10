using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    //public interface IMergeRequest : IBulkRequest
    //{
    //}

    //public interface IMergeRequest<TEntity> : IMergeRequest, IRequest
    //    where TEntity : class
    //{
    //}

    //public interface IMergeRequest<TEntity, TOut> : IMergeRequest, IRequest<MergeResult<TOut>>
    //    where TEntity : class
    //{
    //}

    //public class MergeResult<TOut>
    //{
    //    public List<TOut> Items { get; }

    //    public MergeResult(List<TOut> items)
    //    {
    //        Items = items;
    //    }
    //}

    //[DoNotValidate]
    //public class MergeRequest<TEntity, TIn> : IMergeRequest<TEntity>
    //    where TEntity : class
    //{
    //    public MergeRequest(List<TIn> items) { Items = items; }

    //    public List<TIn> Items { get; }
    //}

    //[DoNotValidate]
    //public class MergeByIdRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
    //    where TEntity : class
    //{
    //    public MergeByIdRequest(List<TIn> items) : base(items) { }
    //}

    //public class MergeByIdRequestProfile<TEntity, TIn>
    //    : CrudRequestProfile<MergeByIdRequest<TEntity, TIn>>
    //    where TEntity : class
    //{
    //    public MergeByIdRequestProfile()
    //    {
    //        ForEntity<TEntity>()
    //            .CreateAllWith(request => request.Items)
    //            .UpdateAllWith(request => request.Items, "Id", "Id");
    //    }
    //}

    //[DoNotValidate]
    //public class MergeByGuidRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
    //    where TEntity : class
    //{
    //    public MergeByGuidRequest(List<TIn> items) : base(items) { }
    //}

    //public class MergeByGuidRequestProfile<TEntity, TIn>
    //    : CrudRequestProfile<MergeByGuidRequest<TEntity, TIn>>
    //    where TEntity : class
    //{
    //    public MergeByGuidRequestProfile()
    //    {
    //        ForEntity<TEntity>()
    //            .CreateAllWith(request => request.Items)
    //            .UpdateAllWith(request => request.Items, "Guid", "Guid");
    //    }
    //}

    //[DoNotValidate]
    //public class MergeRequest<TEntity, TIn, TOut> : IMergeRequest<TEntity, TOut>
    //    where TEntity : class
    //{
    //    public MergeRequest(List<TIn> items) { Items = items; }

    //    public List<TIn> Items { get; }
    //}
    
    //[DoNotValidate]
    //public class MergeByIdRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
    //    where TEntity : class
    //{
    //    public MergeByIdRequest(List<TIn> items) : base(items) { }
    //}

    //public class MergeByIdRequestProfile<TEntity, TIn, TOut>
    //    : CrudRequestProfile<MergeByIdRequest<TEntity, TIn, TOut>>
    //    where TEntity : class
    //{
    //    public MergeByIdRequestProfile()
    //    {
    //        ForEntity<TEntity>()
    //            .CreateAllWith(request => request.Items)
    //            .UpdateAllWith(request => request.Items, "Id", "Id");
    //    }
    //}

    //[DoNotValidate]
    //public class MergeByGuidRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
    //    where TEntity : class
    //{
    //    public MergeByGuidRequest(List<TIn> items) : base(items) { }
    //}

    //public class MergeByGuidRequestProfile<TEntity, TIn, TOut>
    //    : CrudRequestProfile<MergeByGuidRequest<TEntity, TIn, TOut>>
    //    where TEntity : class
    //{
    //    public MergeByGuidRequestProfile()
    //    {
    //        ForEntity<TEntity>()
    //            .CreateAllWith(request => request.Items)
    //            .UpdateAllWith(request => request.Items, "Guid", "Guid");
    //    }
    //}
}
