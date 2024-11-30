using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAll;

public interface IGetAllHandler : IBaseHandler<EmptyRequest, CurrencyRateListResponse>
{
}
