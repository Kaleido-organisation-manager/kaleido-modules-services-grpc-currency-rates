using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Create;

public class CreateManager : ICreateManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifeCycleHandler;

    public CreateManager(IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifeCycleHandler)
    {
        _entityLifeCycleHandler = entityLifeCycleHandler;
    }

    public async Task<ManagerResponse> CreateAsync(CurrencyRateEntity request, CancellationToken cancellationToken = default)
    {
        var entityExists = await _entityLifeCycleHandler.FindAsync(
            entity => entity.OriginKey == request.OriginKey && entity.TargetKey == request.TargetKey,
            revision => revision.Status == RevisionStatus.Active,
            cancellationToken: cancellationToken
        );

        if (entityExists.Any() && entityExists.FirstOrDefault()?.Revision.Action != RevisionAction.Deleted)
        {
            throw new InvalidOperationException("Entity already exists");
        }
        else if (entityExists.Any() && entityExists.FirstOrDefault()?.Revision.Action == RevisionAction.Deleted)
        {
            var result = await _entityLifeCycleHandler.RestoreAsync(
                entityExists.FirstOrDefault()?.Key ?? Guid.Empty,
                cancellationToken: cancellationToken
            );
            return ManagerResponse.Success(result);
        }
        else
        {
            var entity = await _entityLifeCycleHandler.CreateAsync(request, cancellationToken: cancellationToken);
            return ManagerResponse.Success(entity);
        }
    }
}
