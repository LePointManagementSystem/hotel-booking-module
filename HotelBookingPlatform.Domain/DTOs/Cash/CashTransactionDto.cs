using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.DTOs.Cash;

public class CashTransactionDto
{
    public int CashTransactionId { get; set; }
    public int HotelId { get; set; }
    public int CashSessionId { get; set; }

    public string ActorUserId { get; set; } = string.Empty;

    public CashTransactionType Type { get; set; }
    public CurrencyCode Currency { get; set; }
    public decimal Amount { get; set; }

    public string Note { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Reference { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public CashShift Shift { get; set; }
}
