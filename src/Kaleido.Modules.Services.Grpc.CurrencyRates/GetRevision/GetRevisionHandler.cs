using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetRevision;

public class GetRevisionHandler : IGetRevisionHandler
{
    private readonly IGetRevisionManager _getRevisionManager;
    private readonly ILogger<GetRevisionHandler> _logger;
    private readonly IMapper _mapper;
    private readonly KeyValidator _keyValidator;

    public GetRevisionHandler(
        IGetRevisionManager getRevisionManager,
        ILogger<GetRevisionHandler> logger,
        IMapper mapper,
        KeyValidator keyValidator)
    {
        _getRevisionManager = getRevisionManager;
        _logger = logger;
        _mapper = mapper;
        _keyValidator = keyValidator;
    }

    public async Task<CurrencyRateResponse> HandleAsync(CurrencyRateRevisionRequest request, CancellationToken cancellationToken)
    {
        ManagerResponse? result;
        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            var createdAt = request.CreatedAt.ToDateTime();
            result = await _getRevisionManager.GetRevisionAsync(key, createdAt, cancellationToken);
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Invalid request");
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting revision");
            throw new RpcException(new Status(StatusCode.Internal, e.Message));
        }

        if (result is null || result.Value.State == ManagerResponseState.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Revision not found"));
        }

        return _mapper.Map<CurrencyRateResponse>(result.Value.CurrencyRate);
    }
}
