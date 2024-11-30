using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Delete;

public class DeleteManager : IDeleteManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifeCycleHandler;

    public DeleteManager(IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifeCycleHandler)
    {
        _entityLifeCycleHandler = entityLifeCycleHandler;
    }

    public async Task<ManagerResponse> DeleteAsync(Guid key, CancellationToken cancellationToken)
    {
        EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>? entity;
        try
        {
            entity = await _entityLifeCycleHandler.GetAsync(key, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is EntityNotFoundException or RevisionNotFoundException)
        {
            return ManagerResponse.NotFound();
        }

        if (entity == null || entity.Revision.Action == RevisionAction.Deleted)
        {
            return ManagerResponse.NotFound();
        }

        var deletedEntity = await _entityLifeCycleHandler.DeleteAsync(key, cancellationToken: cancellationToken);

        return ManagerResponse.Success(deletedEntity);
    }
}
