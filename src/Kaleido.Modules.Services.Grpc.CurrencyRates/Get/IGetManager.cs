using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Get;

public interface IGetManager
{
    Task<ManagerResponse> GetAsync(Guid key, CancellationToken cancellationToken = default);
}
