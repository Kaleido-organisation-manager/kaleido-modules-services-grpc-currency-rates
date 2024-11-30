using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Update;

public interface IUpdateManager
{
    Task<ManagerResponse> UpdateAsync(Guid key, CurrencyRateEntity currencyRate, CancellationToken cancellationToken = default);
}