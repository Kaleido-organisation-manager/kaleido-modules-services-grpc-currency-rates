using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAll;

public interface IGetAllManager
{
    Task<IEnumerable<ManagerResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
