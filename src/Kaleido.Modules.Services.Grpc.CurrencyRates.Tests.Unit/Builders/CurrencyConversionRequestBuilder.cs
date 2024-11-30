using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;

public class CurrencyConversionRequestBuilder
{
    private readonly CurrencyConversionRequest _request;

    public CurrencyConversionRequestBuilder()
    {
        _request = new CurrencyConversionRequest
        {
            OriginKey = Guid.NewGuid().ToString(),
            TargetKey = Guid.NewGuid().ToString()
        };
    }

    public CurrencyConversionRequestBuilder WithOriginKey(string key)
    {
        _request.OriginKey = key;
        return this;
    }

    public CurrencyConversionRequestBuilder WithOriginKey(Guid key)
    {
        _request.OriginKey = key.ToString();
        return this;
    }

    public CurrencyConversionRequestBuilder WithTargetKey(string key)
    {
        _request.TargetKey = key;
        return this;
    }

    public CurrencyConversionRequestBuilder WithTargetKey(Guid key)
    {
        _request.TargetKey = key.ToString();
        return this;
    }

    public CurrencyConversionRequest Build() => _request;
}