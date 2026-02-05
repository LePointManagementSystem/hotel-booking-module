using HotelBookingPlatform.Domain.DTOs.Cash;

public class OpenCashSessionRequest
{
    public int HotelId { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.HTG;
    public CashShift Shift { get; set; } = CashShift.Morning;
    public decimal OpeningBalance { get; set; }
}