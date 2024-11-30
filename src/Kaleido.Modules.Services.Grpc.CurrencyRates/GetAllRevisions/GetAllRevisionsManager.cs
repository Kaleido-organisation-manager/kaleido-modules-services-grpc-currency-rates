using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllRevisions;

public class GetAllRevisionsManager : IGetAllRevisionsManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;

    public GetAllRevisionsManager(IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler)
    {
        _entityLifecycleHandler = entityLifecycleHandler;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken)
    {
        var result = await _entityLifecycleHandler.GetAllAsync(key, cancellationToken);
        return result.Select(r => ManagerResponse.Success(r));
    }
}
