using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;

public interface IGetAllRevisionsHandler : IBaseHandler<CurrencyRateRequest, CurrencyRateListResponse>
{
}
