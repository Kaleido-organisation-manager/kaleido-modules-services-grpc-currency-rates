using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;

public interface IGetAllConversionsHandler : IBaseHandler<CurrencyRateRequest, CurrencyRateListResponse>
{ }