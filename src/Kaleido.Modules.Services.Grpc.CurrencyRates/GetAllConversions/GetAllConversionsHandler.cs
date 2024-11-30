using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;

public class GetAllConversionsHandler : IGetAllConversionsHandler
{
    private readonly IGetAllConversionsManager _manager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllConversionsHandler> _logger;
    private readonly KeyValidator _keyValidator;

    public GetAllConversionsHandler(
        IGetAllConversionsManager manager,
        IMapper mapper,
        ILogger<GetAllConversionsHandler> logger,
        KeyValidator keyValidator
    )
    {
        _manager = manager;
        _mapper = mapper;
        _logger = logger;
        _keyValidator = keyValidator;
    }

    public async Task<CurrencyRateListResponse> HandleAsync(CurrencyRateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            var result = await _manager.GetAllConversionsAsync(key, cancellationToken);
            return _mapper.Map<CurrencyRateListResponse>(result.Select(r => r.CurrencyRate));
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Invalid key");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }
}
