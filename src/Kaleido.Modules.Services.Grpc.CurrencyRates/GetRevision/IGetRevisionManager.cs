using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetRevision;

public interface IGetRevisionManager
{
    Task<ManagerResponse> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken);
}