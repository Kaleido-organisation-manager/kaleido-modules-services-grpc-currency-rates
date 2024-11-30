using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAllConversions;

public class GetAllConversionsManager : IGetAllConversionsManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;

    public GetAllConversionsManager(
        IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler
    )
    {
        _entityLifecycleHandler = entityLifecycleHandler;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllConversionsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var result = await _entityLifecycleHandler.FindAllAsync(
            entity => entity.OriginKey == key,
            revision => revision.Status == RevisionStatus.Active && revision.Action != RevisionAction.Deleted,
            cancellationToken: cancellationToken);

        return result.Select(r => ManagerResponse.Success(r)).ToList();
    }
}
