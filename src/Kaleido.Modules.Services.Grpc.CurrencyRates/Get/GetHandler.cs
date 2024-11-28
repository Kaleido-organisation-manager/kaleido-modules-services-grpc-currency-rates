using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Get;

public class GetHandler : IGetHandler
{
    private readonly IGetManager _getManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetHandler> _logger;
    private readonly KeyValidator _keyValidator;

    public GetHandler(
        IGetManager getManager,
        IMapper mapper,
        ILogger<GetHandler> logger,
        KeyValidator keyValidator)
    {
        _getManager = getManager;
        _mapper = mapper;
        _logger = logger;
        _keyValidator = keyValidator;
    }

    public async Task<CurrencyRateResponse> HandleAsync(CurrencyRateRequest request, CancellationToken cancellationToken = default)
    {
        ManagerResponse? response;

        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            response = await _getManager.GetAsync(key, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for get currency rate. Key: {Key}. Errors: {Errors}", request.Key, ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currency rate with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }

        if (response == null || response.Value.State == ManagerResponseState.NotFound)
        {
            _logger.LogWarning("Currency rate with key {Key} not found", request.Key);
            throw new RpcException(new Status(StatusCode.NotFound, "Currency rate not found"));
        }

        return _mapper.Map<CurrencyRateResponse>(response.Value.CurrencyRate);
    }
}
