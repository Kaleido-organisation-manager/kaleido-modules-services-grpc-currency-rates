using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;

public class CurrencyRateRequestBuilder
{
    private readonly CurrencyRateRequest _request;

    public CurrencyRateRequestBuilder()
    {
        _request = new CurrencyRateRequest
        {
            Key = Guid.NewGuid().ToString()
        };
    }

    public CurrencyRateRequestBuilder WithKey(string key)
    {
        _request.Key = key;
        return this;
    }

    public CurrencyRateRequestBuilder WithKey(Guid key)
    {
        _request.Key = key.ToString();
        return this;
    }

    public CurrencyRateRequest Build() => _request;
}