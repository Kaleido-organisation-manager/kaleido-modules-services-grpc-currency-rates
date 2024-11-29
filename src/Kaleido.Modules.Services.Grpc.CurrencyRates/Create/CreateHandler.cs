using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Create;

public class CreateHandler : ICreateHandler
{
    private readonly ICreateManager _createManager;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateHandler> _logger;
    private readonly CurrencyRateValidator _validator;

    public CreateHandler(
        ICreateManager createManager,
        IMapper mapper,
        ILogger<CreateHandler> logger,
        CurrencyRateValidator validator)
    {
        _createManager = createManager;
        _mapper = mapper;
        _logger = logger;
        _validator = validator;
    }

    public async Task<CurrencyRateResponse> HandleAsync(CurrencyRate request, CancellationToken cancellationToken)
    {
        try
        {
            await _validator.ValidateAndThrowAsync(request);
            var currencyRate = _mapper.Map<CurrencyRateEntity>(request);
            var managerResponse = await _createManager.CreateAsync(currencyRate, cancellationToken);
            return _mapper.Map<CurrencyRateResponse>(managerResponse.CurrencyRate);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error creating currency rate");
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating currency rate");
            throw new RpcException(new Status(StatusCode.Internal, "Error creating currency rate", ex));
        }
    }
}