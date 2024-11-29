using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Update;

public interface IUpdateHandler : IBaseHandler<CurrencyRateActionRequest, CurrencyRateResponse>
{
}