using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

public readonly struct ManagerResponse
{
    public readonly ManagerResponseState State = ManagerResponseState.Success;
    public readonly EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity>? CurrencyRate;

    public ManagerResponse(EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> currencyRate)
    {
        CurrencyRate = currencyRate;
    }

    public ManagerResponse(ManagerResponseState state)
    {
        State = state;
        CurrencyRate = null;
    }

    public static ManagerResponse Success(EntityLifeCycleResult<CurrencyRateEntity, BaseRevisionEntity> currencyRate)
    {
        return new ManagerResponse(currencyRate);
    }

    public static ManagerResponse NotFound()
    {
        return new ManagerResponse(ManagerResponseState.NotFound);
    }
}
