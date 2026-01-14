using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.Entities;

/// <summary>
/// Petty cash journal entry (IN/OUT) for an hotel.
/// This is an audit record and should not be deleted silently.
/// </summary>
public class CashTransaction
{
    public int CashTransactionID { get; set; }

    public int HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;

    /// <summary>
    /// Actor (staff/admin) who created the transaction.
    /// </summary>
    public string ActorUserId { get; set; } = string.Empty;
    public LocalUser ActorUser { get; set; } = null!;

    public CashTransactionType Type { get; set; }
    public CurrencyCode Currency { get; set; }

    public decimal Amount { get; set; }

    /// <summary>
    /// Required for OUT transactions. Optional for IN.
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Optional: category such as "Supplies", "Maintenance", etc.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Optional external reference (invoice number, receipt number, etc.)
    /// </summary>
    public string? Reference { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
