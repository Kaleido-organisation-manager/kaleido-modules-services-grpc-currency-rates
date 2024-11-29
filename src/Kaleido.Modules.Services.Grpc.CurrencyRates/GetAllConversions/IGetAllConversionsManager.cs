using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;

public interface IGetAllConversionsManager
{
    Task<IEnumerable<ManagerResponse>> GetAllConversionsAsync(Guid key, CancellationToken cancellationToken = default);
}
