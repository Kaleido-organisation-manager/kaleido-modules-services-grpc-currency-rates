using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Get;

public class GetManager : IGetManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;

    public GetManager(
        IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler
    )
    {
        _entityLifecycleHandler = entityLifecycleHandler;
    }

    public async Task<ManagerResponse> GetAsync(Guid key, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _entityLifecycleHandler.GetAsync(key, cancellationToken: cancellationToken);
            return result == null || result.Revision.Action == RevisionAction.Deleted ?
                ManagerResponse.NotFound() :
                ManagerResponse.Success(result);
        }
        catch (Exception ex) when (ex is EntityNotFoundException or RevisionNotFoundException)
        {
            return ManagerResponse.NotFound();
        }
    }
}