namespace Turner.Infrastructure.Crud.Configuration
{
    public class RequestOptions
    {
        public bool SuppressCreateActionsInSave { get; set; } = false;
        public bool SuppressUpdateActionsInSave { get; set; } = false;

        public RequestOptions Clone()
        {
            return (RequestOptions) MemberwiseClone();
        }
    }
}
