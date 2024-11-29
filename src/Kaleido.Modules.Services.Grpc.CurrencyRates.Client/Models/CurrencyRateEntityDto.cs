namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Models;

/// <summary>
/// Data transfer object for currency rate information
/// </summary>
public class CurrencyRateEntityDto
{
    /// <summary>
    /// The unique identifier of the currency rate
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// The key of the origin currency
    /// </summary>
    public Guid OriginKey { get; set; }

    /// <summary>
    /// The key of the target currency
    /// </summary>
    public Guid TargetKey { get; set; }

    /// <summary>
    /// The conversion rate
    /// </summary>
    public decimal Rate { get; set; }
}