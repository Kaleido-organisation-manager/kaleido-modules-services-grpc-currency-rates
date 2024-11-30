using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;

public interface IDeleteManager
{
    Task<ManagerResponse> DeleteAsync(Guid key, CancellationToken cancellationToken);
}
