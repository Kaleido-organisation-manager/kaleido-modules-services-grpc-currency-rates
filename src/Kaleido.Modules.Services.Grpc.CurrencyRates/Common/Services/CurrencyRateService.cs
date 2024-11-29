using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Create;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Update;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Services;

public class CurrencyRateService : GrpcCurrencyRateService.GrpcCurrencyRateServiceBase
{
    private readonly ICreateHandler _createHandler;
    private readonly IDeleteHandler _deleteHandler;
    private readonly IUpdateHandler _updateHandler;

    public CurrencyRateService(
        ICreateHandler createHandler,
        IDeleteHandler deleteHandler,
        IUpdateHandler updateHandler)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _updateHandler = updateHandler;
    }

    public override async Task<CurrencyRateResponse> CreateCurrencyRate(CurrencyRate request, ServerCallContext context)
    {
        return await _createHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyRateResponse> DeleteCurrencyRate(CurrencyRateRequest request, ServerCallContext context)
    {
        return await _deleteHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyRateResponse> UpdateCurrencyRate(CurrencyRateActionRequest request, ServerCallContext context)
    {
        return await _updateHandler.HandleAsync(request, context.CancellationToken);
    }
}
