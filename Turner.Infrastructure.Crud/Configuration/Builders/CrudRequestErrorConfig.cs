using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public class CrudRequestErrorConfig
    {
        public bool? FailedToFindInGetIsError { get; set; }

        public bool? FailedToFindInUpdateIsError { get; set; }

        public bool? FailedToFindInAnyIsError
        {
            get
            {
                return new[]
                {
                    FailedToFindInGetIsError,
                    FailedToFindInUpdateIsError
                }.Any(x => x.HasValue && x.Value);
            }

            set
            {
                FailedToFindInGetIsError = 
                FailedToFindInUpdateIsError = value;
            }
        }
    }
}
