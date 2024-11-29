using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Create;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.CurrencyRates.GetConversion;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Update;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Services;

public class CurrencyRateService : GrpcCurrencyRateService.GrpcCurrencyRateServiceBase
{
    private readonly ICreateHandler _createHandler;
    private readonly IDeleteHandler _deleteHandler;
    private readonly IUpdateHandler _updateHandler;
    private readonly IGetAllConversionsHandler _getAllConversionsHandler;
    private readonly IGetAllRevisionsHandler _getAllRevisionsHandler;
    private readonly IGetConversionHandler _getConversionHandler;

    public CurrencyRateService(
        ICreateHandler createHandler,
        IDeleteHandler deleteHandler,
        IUpdateHandler updateHandler,
        IGetAllConversionsHandler getAllConversionsHandler,
        IGetAllRevisionsHandler getAllRevisionsHandler,
        IGetConversionHandler getConversionHandler)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _updateHandler = updateHandler;
        _getAllConversionsHandler = getAllConversionsHandler;
        _getAllRevisionsHandler = getAllRevisionsHandler;
        _getConversionHandler = getConversionHandler;
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

    public override async Task<CurrencyRateListResponse> GetAllCurrencyConversions(CurrencyRateRequest request, ServerCallContext context)
    {
        return await _getAllConversionsHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyRateListResponse> GetAllCurrencyRateRevisions(CurrencyRateRequest request, ServerCallContext context)
    {
        return await _getAllRevisionsHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyRateResponse> GetCurrencyConversion(CurrencyConversionRequest request, ServerCallContext context)
    {
        return await _getConversionHandler.HandleAsync(request, context.CancellationToken);
    }
}
