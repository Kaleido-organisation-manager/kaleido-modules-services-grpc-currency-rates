using System.Text.Json;
using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetConversion;

public class GetConversionHandler : IGetConversionHandler
{
    private readonly IGetConversionManager _getConversionManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetConversionHandler> _logger;
    private readonly KeyValidator _keyValidator;

    public GetConversionHandler(
        IGetConversionManager getConversionManager,
        IMapper mapper,
        ILogger<GetConversionHandler> logger,
        KeyValidator keyValidator
    )
    {
        _getConversionManager = getConversionManager;
        _mapper = mapper;
        _logger = logger;
        _keyValidator = keyValidator;
    }

    public async Task<CurrencyRateResponse> HandleAsync(CurrencyConversionRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("request: {request}", JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true }));
        ManagerResponse? result;
        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.OriginKey, cancellationToken);
            await _keyValidator.ValidateAndThrowAsync(request.TargetKey, cancellationToken);
            var originKey = Guid.Parse(request.OriginKey);
            var targetKey = Guid.Parse(request.TargetKey);

            result = await _getConversionManager.GetConversionAsync(originKey, targetKey, cancellationToken);
            _logger.LogInformation("result: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Invalid request");
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting conversion");
            throw new RpcException(new Status(StatusCode.Internal, e.Message));
        }

        if (result is null || result.Value.State == ManagerResponseState.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Conversion not found"));
        }

        return _mapper.Map<CurrencyRateResponse>(result.Value.CurrencyRate);
    }
}
