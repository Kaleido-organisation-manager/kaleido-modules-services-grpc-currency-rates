using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetRevision;

public class GetRevisionManager : IGetRevisionManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;
    public GetRevisionManager(IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler)
    {
        _entityLifecycleHandler = entityLifecycleHandler;
    }

    public async Task<ManagerResponse> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _entityLifecycleHandler.GetHistoricAsync(key, createdAt, cancellationToken);

            return result is null ? ManagerResponse.NotFound() : ManagerResponse.Success(result);
        }
        catch (Exception e) when (e is RevisionNotFoundException or EntityNotFoundException)
        {
            return ManagerResponse.NotFound();
        }
    }
}