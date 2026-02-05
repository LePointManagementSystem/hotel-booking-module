using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.DTOs.Cash;

public class CreateCashTransactionRequest
{
    /// <summary>
    /// Hotel identifier. For Staff users, this will be ignored and replaced
    /// by the hotelId claim (scoping).
    /// </summary>
    public int HotelId { get; set; }

    public CashTransactionType Type { get; set; }
    public CurrencyCode Currency { get; set; }
    public decimal Amount { get; set; }

    /// <summary>
    /// Required for OUT transactions.
    /// </summary>
    public string Note { get; set; } = string.Empty;

    public string? Category { get; set; }
    public string? Reference { get; set; }

    public CashShift Shift { get; set; } = CashShift.Morning;
    public int? CashSessionId { get; set; }
}
