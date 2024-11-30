using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;

public class GetAllRevisionsHandler : IGetAllRevisionsHandler
{
    private readonly IGetAllRevisionsManager _getAllRevisionsManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllRevisionsHandler> _logger;
    private readonly KeyValidator _keyValidator;

    public GetAllRevisionsHandler(
        IGetAllRevisionsManager getAllRevisionsManager,
        IMapper mapper,
        ILogger<GetAllRevisionsHandler> logger,
        KeyValidator keyValidator)
    {
        _getAllRevisionsManager = getAllRevisionsManager;
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
            var result = await _getAllRevisionsManager.GetAllRevisionsAsync(key, cancellationToken);
            return _mapper.Map<CurrencyRateListResponse>(result.Select(r => r.CurrencyRate));
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error getting all revisions");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all revisions");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
