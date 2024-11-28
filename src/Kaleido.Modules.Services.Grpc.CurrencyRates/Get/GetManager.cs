using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Get;

public class GetManager : IGetManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;

    public GetManager(IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler)
    {
        _entityLifecycleHandler = entityLifecycleHandler;
    }

    public async Task<ManagerResponse> GetAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var result = await _entityLifecycleHandler.GetAsync(key, cancellationToken: cancellationToken);
        if (result == null || result.Revision.Action == RevisionAction.Deleted)
        {
            return ManagerResponse.NotFound();
        }
        return ManagerResponse.Success(result);
    }
}
