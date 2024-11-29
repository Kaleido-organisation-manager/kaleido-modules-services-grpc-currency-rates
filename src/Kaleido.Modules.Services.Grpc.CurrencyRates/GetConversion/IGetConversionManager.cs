using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetConversion;

public interface IGetConversionManager
{
    Task<ManagerResponse> GetConversionAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken);
}
