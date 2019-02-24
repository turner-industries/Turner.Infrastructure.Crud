using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Validation
{
    public interface IRequestValidator<TRequest>
    {
        Task<List<ValidationError>> ValidateAsync(TRequest request, CancellationToken token = default(CancellationToken));
    }
}
