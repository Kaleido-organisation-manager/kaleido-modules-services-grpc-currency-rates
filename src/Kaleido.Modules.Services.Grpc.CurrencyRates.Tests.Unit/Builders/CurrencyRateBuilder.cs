using Kaleido.Grpc.CurrencyRates;
using Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;

public class CurrencyRateBuilder
{
    private readonly CurrencyRate _currencyRate;

    public CurrencyRateBuilder()
    {
        _currencyRate = new CurrencyRate
        {
            OriginKey = Guid.NewGuid().ToString(),
            TargetKey = Guid.NewGuid().ToString(),
            Rate = 0.85,
        };
    }

    public CurrencyRate Build() => _currencyRate;

    public CurrencyRateEntity BuildEntity() => new()
    {
        OriginKey = Guid.Parse(_currencyRate.OriginKey),
        TargetKey = Guid.Parse(_currencyRate.TargetKey),
        Rate = Math.Round((decimal)_currencyRate.Rate, 2, MidpointRounding.AwayFromZero),
    };
}