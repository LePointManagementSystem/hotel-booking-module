using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.DTOs.Reports;

public sealed class CashMonthlySummaryDto
{
    public CurrencyCode Currency { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal Net => TotalIn - TotalOut;
}
