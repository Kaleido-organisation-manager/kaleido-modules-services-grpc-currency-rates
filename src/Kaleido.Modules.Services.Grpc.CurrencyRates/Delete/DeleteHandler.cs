using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;

public class DeleteHandler : IDeleteHandler
{
    private readonly IDeleteManager _deleteManager;
    private readonly KeyValidator _keyValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<DeleteHandler> _logger;

    public DeleteHandler(
        IDeleteManager deleteManager,
        KeyValidator keyValidator,
        IMapper mapper,
        ILogger<DeleteHandler> logger)
    {
        _deleteManager = deleteManager;
        _keyValidator = keyValidator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyRateResponse> HandleAsync(CurrencyRateRequest request, CancellationToken cancellationToken = default)
    {
        ManagerResponse? response;
        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key);
            var key = Guid.Parse(request.Key);
            response = await _deleteManager.DeleteAsync(key, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Error deleting currency rate");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting currency rate");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error", ex));
        }

        if (response == null || response.Value.State == ManagerResponseState.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Currency rate not found"));
        }

        return _mapper.Map<CurrencyRateResponse>(response.Value.CurrencyRate);
    }
}
