using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Get;

public interface IGetHandler : IBaseHandler<CurrencyRateRequest, CurrencyRateResponse>
{
}
