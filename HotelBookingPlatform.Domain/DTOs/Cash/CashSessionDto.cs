using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.DTOs.Cash;

public class CashSessionDto
{
    public int CashSessionId { get; set; }
    public int HotelId { get; set; }

    public CurrencyCode Currency { get; set; }
    public CashShift Shift { get; set; }

    public decimal OpeningBalance { get; set; }
    public DateTime OpenedAtUtc { get; set; }

    // ✅ Add these (to fix your compile errors)
    public decimal? ClosingCounted { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    public decimal Expected { get; set; }
    public decimal Difference { get; set; }

    public bool IsClosed { get; set; }
}
