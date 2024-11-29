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
        KeyValidator keyValidator
    )
    {
        _getManager = getManager;
        _mapper = mapper;
        _logger = logger;
        _keyValidator = keyValidator;
    }

    public async Task<CurrencyRateResponse> HandleAsync(CurrencyRateRequest request, CancellationToken cancellationToken = default)
    {
        ManagerResponse? result;
        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key);
            var key = Guid.Parse(request.Key);
            result = await _getManager.GetAsync(key, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Invalid key format");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid key format", ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currency rate");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting currency rate", ex));
        }

        if (result == null || result.Value.State == ManagerResponseState.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Currency rate not found"));
        }

        return _mapper.Map<CurrencyRateResponse>(result.Value.CurrencyRate);
    }
}