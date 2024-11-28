using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;

public interface IDeleteHandler : IBaseHandler<CurrencyRateRequest, CurrencyRateResponse>
{
}
