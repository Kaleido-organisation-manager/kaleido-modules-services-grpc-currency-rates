using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Common.Models;

public class CurrencyRateEntity : BaseEntity
{
    public Guid OriginKey { get; set; }
    public Guid TargetKey { get; set; }
    public decimal Rate { get; set; }
}