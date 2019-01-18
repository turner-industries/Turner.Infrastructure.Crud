using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Validation
{
    public interface IValidator<TRequest>
    {
        Task<List<Error>> ValidateAsync(TRequest request, CancellationToken token = default(CancellationToken));
    }
}
