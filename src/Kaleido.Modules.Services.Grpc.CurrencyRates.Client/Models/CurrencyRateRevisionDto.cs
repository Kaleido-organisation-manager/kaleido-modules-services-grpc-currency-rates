using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.CurrencyRates.Client.Models;

/// <summary>
/// Data transfer object for revision information
/// </summary>
public class CurrencyRateRevisionDto
{
    /// <summary>
    /// The unique identifier of the revision
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// The revision number
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// The action performed in this revision
    /// </summary>
    public RevisionAction Action { get; set; }

    /// <summary>
    /// When this revision was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The status of this revision
    /// </summary>
    public RevisionStatus Status { get; set; }
}