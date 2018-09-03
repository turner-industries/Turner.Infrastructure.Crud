namespace Turner.Infrastructure.Crud.Configuration
{
    public class ErrorConfig
    {
        public bool FailedToFindInGetIsError { get; set; } = true;
        public bool FailedToFindInUpdateIsError { get; set; } = true;
        public bool FailedToFindInDeleteIsError { get; set; } = false;
    }
}
