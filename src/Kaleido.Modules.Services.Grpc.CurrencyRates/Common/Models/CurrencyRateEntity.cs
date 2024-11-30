using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

public class CurrencyRateEntity : BaseEntity
{
    public Guid OriginKey { get; set; }
    public Guid TargetKey { get; set; }
    public decimal Rate { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is CurrencyRateEntity other &&
            OriginKey == other.OriginKey &&
            TargetKey == other.TargetKey &&
            Rate == other.Rate;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), OriginKey, TargetKey, Rate);
    }
}
