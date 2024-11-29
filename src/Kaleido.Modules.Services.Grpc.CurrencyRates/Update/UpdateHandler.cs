using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Update;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUpdateManager _updateManager;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IMapper _mapper;
    private readonly CurrencyRateValidator _currencyRateValidator;
    private readonly KeyValidator _keyValidator;

    public UpdateHandler(
        IUpdateManager updateManager,
        ILogger<UpdateHandler> logger,
        IMapper mapper,
        CurrencyRateValidator currencyRateValidator,
        KeyValidator keyValidator)
    {
        _updateManager = updateManager;
        _logger = logger;
        _mapper = mapper;
        _currencyRateValidator = currencyRateValidator;
        _keyValidator = keyValidator;
    }

    public async Task<CurrencyRateResponse> HandleAsync(CurrencyRateActionRequest request, CancellationToken cancellationToken = default)
    {
        ManagerResponse? managerResponse;

        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            await _currencyRateValidator.ValidateAndThrowAsync(request.CurrencyRate, cancellationToken);
            var currencyRate = _mapper.Map<CurrencyRateEntity>(request.CurrencyRate);
            managerResponse = await _updateManager.UpdateAsync(key, currencyRate, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Invalid currency rate");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency rate");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        if (managerResponse == null || managerResponse.Value.State == ManagerResponseState.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Currency rate not found"));
        }

        if (managerResponse.Value.State == ManagerResponseState.NotModified)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Currency rate is up to date"));
        }

        var response = _mapper.Map<CurrencyRateResponse>(managerResponse.Value.CurrencyRate);
        return response;
    }
}