using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Create;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Services;

public class CurrencyRateService : GrpcCurrencyRateService.GrpcCurrencyRateServiceBase
{
    private readonly ICreateHandler _createHandler;

    public CurrencyRateService(ICreateHandler createHandler)
    {
        _createHandler = createHandler;
    }

    public override async Task<CurrencyRateResponse> CreateCurrencyRate(CurrencyRate request, ServerCallContext context)
    {
        return await _createHandler.HandleAsync(request, context.CancellationToken);
    }
}
