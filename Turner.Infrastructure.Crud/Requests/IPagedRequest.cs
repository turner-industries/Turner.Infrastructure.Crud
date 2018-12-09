namespace Turner.Infrastructure.Crud.Requests
{
    public interface IPagedRequest : ICrudRequest
    {
        int PageNumber { get; set; }

        int PageSize { get; set; }
    }
}
