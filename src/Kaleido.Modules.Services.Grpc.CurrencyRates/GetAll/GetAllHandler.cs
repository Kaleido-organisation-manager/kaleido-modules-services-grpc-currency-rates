using AutoMapper;
using Grpc.Core;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAll;

public class GetAllHandler : IGetAllHandler
{
    private readonly IGetAllManager _getAllManager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllHandler> _logger;

    public GetAllHandler(
        IGetAllManager getAllManager,
        IMapper mapper,
        ILogger<GetAllHandler> logger
        )
    {
        _getAllManager = getAllManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyRateListResponse> HandleAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _getAllManager.GetAllAsync(cancellationToken);
            return _mapper.Map<CurrencyRateListResponse>(result.Select(x => x.CurrencyRate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all currency rates");
            throw new RpcException(new Status(StatusCode.Internal, "Error getting all currency rates", ex));
        }
    }
}
