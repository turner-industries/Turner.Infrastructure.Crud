namespace Turner.Infrastructure.Crud.Configuration
{
    public class RequestOptions
    {
        public bool SuppressCreateActionsInSave { get; set; } = false;
        public bool SuppressUpdateActionsInSave { get; set; } = false;
        public bool UseProjection { get; set; } = true;

        public RequestOptions Clone()
        {
            return (RequestOptions) MemberwiseClone();
        }
    }
}
