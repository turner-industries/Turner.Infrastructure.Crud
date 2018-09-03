using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public class CrudRequestErrorConfig
    {
        public bool? FailedToFindInGetIsError { get; set; }

        public bool? FailedToFindInUpdateIsError { get; set; }

        public bool? FailedToFindInDeleteIsError { get; set; }

        public bool? FailedToFindInAnyIsError
        {
            get
            {
                return new[]
                {
                    FailedToFindInGetIsError,
                    FailedToFindInUpdateIsError,
                    FailedToFindInDeleteIsError
                }.Any(x => x.HasValue && x.Value);
            }

            set
            {
                FailedToFindInGetIsError = 
                FailedToFindInUpdateIsError =
                FailedToFindInDeleteIsError = value;
            }
        }
    }
}
