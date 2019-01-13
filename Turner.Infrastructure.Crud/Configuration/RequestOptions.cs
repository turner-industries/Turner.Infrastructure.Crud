namespace Turner.Infrastructure.Crud.Configuration
{
    public class RequestOptions
    {
        public bool UseProjection { get; set; } = true;

        public RequestOptions Clone()
        {
            return (RequestOptions) MemberwiseClone();
        }
    }
}
