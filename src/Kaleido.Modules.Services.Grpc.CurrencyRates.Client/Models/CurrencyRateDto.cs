namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Models;

/// <summary>
/// Data transfer object for currency rate information
/// </summary>
public class CurrencyRateDto
{
    /// <summary>
    /// The unique identifier of the currency rate
    /// </summary>
    public Guid Key { get; set; }
    /// <summary>
    /// The entity details of the currency rate
    /// </summary>
    public CurrencyRateEntityDto Entity { get; set; } = new();
    /// <summary>
    /// The revision details of the currency rate
    /// </summary>
    public CurrencyRateRevisionDto Revision { get; set; } = new();
}