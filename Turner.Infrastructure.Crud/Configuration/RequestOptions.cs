namespace Turner.Infrastructure.Crud.Configuration
{
    public class RequestOptions
    {
        public bool SuppressCreateActionsInSave { get; set; }

        public bool SuppressUpdateActionsInSave { get; set; }

        public bool UseProjection { get; set; } = true;

        public RequestOptions Clone()
        {
            return (RequestOptions) MemberwiseClone();
        }
    }
}
