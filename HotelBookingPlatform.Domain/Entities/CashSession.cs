using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.Entities;

public class CashSession
{
    public int CashSessionId { get; set; }

    public int HotelId { get; set; }
    public string OpenedByUserId { get; set; } = string.Empty;

    public CurrencyCode Currency { get; set; } = CurrencyCode.HTG;
    public CashShift Shift { get; set; } = CashShift.Morning;

    public decimal OpeningBalance { get; set; }
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal? ClosingCounted { get; set; }
    public string? ClosedByUserId { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    public bool IsClosed => ClosedAtUtc.HasValue;

    //Navigation
    public ICollection<CashTransaction>Transactions { get; set; } = new List<CashTransaction>();
}