using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Create;

public interface ICreateHandler : IBaseHandler<CurrencyRate, CurrencyRateResponse>
{
}
