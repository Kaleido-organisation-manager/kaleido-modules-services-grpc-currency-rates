using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetConversion;

public interface IGetConversionHandler : IBaseHandler<CurrencyConversionRequest, CurrencyRateResponse>
{
}
