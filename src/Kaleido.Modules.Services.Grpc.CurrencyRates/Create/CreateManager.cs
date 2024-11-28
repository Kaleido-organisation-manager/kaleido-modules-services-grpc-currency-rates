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

    public async Task<ManagerResponse> CreateAsync(CurrencyRateEntity request)
    {
        var entity = await _entityLifeCycleHandler.CreateAsync(request);
        return ManagerResponse.Success(entity);
    }
}
