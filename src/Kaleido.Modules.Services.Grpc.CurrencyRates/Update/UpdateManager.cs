using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Update;

public class UpdateManager : IUpdateManager
{
    private readonly IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> _entityLifecycleHandler;
    private readonly ILogger<UpdateManager> _logger;

    public UpdateManager(
        IEntityLifecycleHandler<CurrencyRateEntity, BaseRevisionEntity> entityLifecycleHandler,
        ILogger<UpdateManager> logger)
    {
        _entityLifecycleHandler = entityLifecycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> UpdateAsync(Guid key, CurrencyRateEntity currencyRate, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _entityLifecycleHandler.UpdateAsync(key, currencyRate, cancellationToken: cancellationToken);
            return ManagerResponse.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Currency rate update is marked as invalid");
            return ManagerResponse.NotFound();
        }
        catch (NotModifiedException ex)
        {
            _logger.LogWarning(ex, "Currency rate is up to date");
            return ManagerResponse.NotModified();
        }
        catch (Exception ex) when (ex is EntityNotFoundException or RevisionNotFoundException)
        {
            _logger.LogWarning(ex, "Currency rate not found");
            return ManagerResponse.NotFound();
        }
    }
}