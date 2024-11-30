using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetRevision;

public interface IGetRevisionHandler : IBaseHandler<CurrencyRateRevisionRequest, CurrencyRateResponse>
{
}