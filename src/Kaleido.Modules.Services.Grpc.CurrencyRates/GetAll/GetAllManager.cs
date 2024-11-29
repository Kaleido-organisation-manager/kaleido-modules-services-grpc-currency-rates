using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.GetAll;

public class GetAllManager : IGetAllManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;

    public GetAllManager(
        IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler
        )
    {
        _entityLifecycleHandler = entityLifecycleHandler;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _entityLifecycleHandler.GetAllAsync(cancellationToken: cancellationToken);

        return result.Select(x => ManagerResponse.Success(x));
    }
}
