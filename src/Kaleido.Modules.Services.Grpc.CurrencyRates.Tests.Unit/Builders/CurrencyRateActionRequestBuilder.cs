using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;

public class CurrencyRateActionRequestBuilder
{
    private readonly CurrencyRateActionRequest _request;

    public CurrencyRateActionRequestBuilder()
    {
        _request = new CurrencyRateActionRequest
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyRate = new CurrencyRateBuilder().Build()
        };
    }

    public CurrencyRateActionRequestBuilder WithKey(string key)
    {
        _request.Key = key;
        return this;
    }

    public CurrencyRateActionRequestBuilder WithKey(Guid key)
    {
        _request.Key = key.ToString();
        return this;
    }

    public CurrencyRateActionRequestBuilder WithCurrencyRate(CurrencyRate currencyRate)
    {
        _request.CurrencyRate = currencyRate;
        return this;
    }

    public CurrencyRateActionRequest Build() => _request;
}