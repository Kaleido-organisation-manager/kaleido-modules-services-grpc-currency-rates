using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetConversion;

public class GetConversionManager : IGetConversionManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;

    public GetConversionManager(
        IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler
        )
    {
        _entityLifecycleHandler = entityLifecycleHandler;
    }

    public async Task<ManagerResponse> GetConversionAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _entityLifecycleHandler.FindAsync(
                entity => entity.OriginKey == originKey && entity.TargetKey == targetKey,
                revision => revision.Action != RevisionAction.Deleted && revision.Status == RevisionStatus.Active,
                cancellationToken: cancellationToken
            );

            var resolvedEntity = result.SingleOrDefault();

            return resolvedEntity is null ? ManagerResponse.NotFound() : ManagerResponse.Success(resolvedEntity);
        }
        catch (RevisionNotFoundException)
        {
            return ManagerResponse.NotFound();
        }
    }
}
