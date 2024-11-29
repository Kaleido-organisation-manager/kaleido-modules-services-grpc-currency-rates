using Google.Protobuf.WellKnownTypes;
using Kaleido.Grpc.CurrencyRates;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Tests.Unit.Builders;

public class CurrencyRateRevisionRequestBuilder
{
    private readonly CurrencyRateRevisionRequest _request;

    public CurrencyRateRevisionRequestBuilder()
    {
        _request = new CurrencyRateRevisionRequest
        {
            Key = Guid.NewGuid().ToString(),
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };
    }

    public CurrencyRateRevisionRequestBuilder WithKey(string key)
    {
        _request.Key = key;
        return this;
    }

    public CurrencyRateRevisionRequestBuilder WithKey(Guid key)
    {
        _request.Key = key.ToString();
        return this;
    }

    public CurrencyRateRevisionRequestBuilder WithCreatedAt(DateTime createdAt)
    {
        _request.CreatedAt = Timestamp.FromDateTime(createdAt.ToUniversalTime());
        return this;
    }

    public CurrencyRateRevisionRequest Build() => _request;
}