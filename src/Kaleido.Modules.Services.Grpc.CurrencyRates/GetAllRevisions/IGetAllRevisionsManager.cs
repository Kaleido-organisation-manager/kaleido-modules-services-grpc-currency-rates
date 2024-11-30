using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;

public interface IGetAllRevisionsManager
{
    Task<IEnumerable<ManagerResponse>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken);
}
