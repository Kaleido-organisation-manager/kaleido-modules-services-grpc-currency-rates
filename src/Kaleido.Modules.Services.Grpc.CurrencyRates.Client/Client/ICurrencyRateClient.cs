using Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Client;

/// <summary>
/// Client interface for interacting with the CurrencyRate service
/// </summary>
public interface ICurrencyRateClient
{
    /// <summary>
    /// Creates a new currency rate
    /// </summary>
    /// <param name="originKey">The key of the origin currency</param>
    /// <param name="targetKey">The key of the target currency</param>
    /// <param name="rate">The conversion rate</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The created currency rate details</returns>
    Task<CurrencyRateDto> CreateAsync(Guid originKey, Guid targetKey, decimal rate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing currency rate
    /// </summary>
    /// <param name="originKey">The new origin currency key</param>
    /// <param name="targetKey">The new target currency key</param>
    /// <param name="rate">The new conversion rate</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The updated currency rate details</returns>
    Task<CurrencyRateDto> UpdateAsync(Guid originKey, Guid targetKey, decimal rate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing currency rate
    /// </summary>
    /// <param name="key">The key of the currency rate to update</param>
    /// <param name="rate">The new conversion rate</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The updated currency rate details</returns>
    Task<CurrencyRateDto> UpdateAsync(Guid key, decimal rate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a currency rate
    /// </summary>
    /// <param name="key">The key of the currency rate to delete</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The deleted currency rate details</returns>
    Task<CurrencyRateDto> DeleteAsync(Guid key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a currency rate
    /// </summary>
    /// <param name="originKey">The key of the origin currency</param>
    /// <param name="targetKey">The key of the target currency</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The deleted currency rate details</returns>
    Task<CurrencyRateDto> DeleteAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific currency rate
    /// </summary>
    /// <param name="key">The key of the currency rate</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The currency rate details</returns>
    Task<CurrencyRateDto> GetAsync(Guid key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currency conversions for a specific currency
    /// </summary>
    /// <param name="currencyKey">The key of the currency to get conversions for</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>List of currency rate details</returns>
    Task<IEnumerable<CurrencyRateDto>> GetAllConversionsAsync(Guid currencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all revisions for a specific currency rate
    /// </summary>
    /// <param name="key">The key of the currency rate</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>List of currency rate revisions</returns>
    Task<IEnumerable<CurrencyRateDto>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all revisions for a specific currency conversion
    /// </summary>
    /// <param name="originKey">The key of the origin currency</param>
    /// <param name="targetKey">The key of the target currency</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>List of currency rate revisions</returns>
    Task<IEnumerable<CurrencyRateDto>> GetAllRevisionsAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific currency conversion
    /// </summary>
    /// <param name="originKey">The key of the origin currency</param>
    /// <param name="targetKey">The key of the target currency</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The currency rate details</returns>
    Task<CurrencyRateDto> GetConversionAsync(Guid originKey, Guid targetKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific revision of a currency rate
    /// </summary>
    /// <param name="key">The key of the currency rate</param>
    /// <param name="createdAt">The timestamp of the revision</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The currency rate revision details</returns>
    Task<CurrencyRateDto> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific revision of a currency conversion
    /// </summary>
    /// <param name="originKey">The key of the origin currency</param>
    /// <param name="targetKey">The key of the target currency</param>
    /// <param name="createdAt">The timestamp of the revision</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The currency rate revision details</returns>
    Task<CurrencyRateDto> GetRevisionAsync(Guid originKey, Guid targetKey, DateTime createdAt, CancellationToken cancellationToken = default);
}