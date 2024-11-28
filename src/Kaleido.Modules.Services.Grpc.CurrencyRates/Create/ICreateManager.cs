using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Create;

public interface ICreateManager
{
    Task<ManagerResponse> CreateAsync(CurrencyRateEntity request);
}
