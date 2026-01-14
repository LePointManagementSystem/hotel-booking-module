using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.DTOs.Reports;

public sealed class MonthlyHotelReportDto
{
    public int HotelId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }

    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public int DaysInMonth { get; set; }

    // Inventory
    public int TotalRooms { get; set; }

    // Occupancy
    public decimal OccupiedRoomNights { get; set; }
    public decimal AvailableRoomNights { get; set; }
    public decimal OccupancyRate { get; set; }

    // Bookings
    public int BookingsCreatedCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }

    // Revenue (based on bookings created in the period)
    public decimal RevenueTotal { get; set; }
    public decimal RevenueCompleted { get; set; }

    // Cash journal summary (petty cash)
    public List<CashMonthlySummaryDto> CashSummary { get; set; } = new();

    // Optional: top cancellation reasons for the period
    public List<CancellationReasonCountDto> TopCancellationReasons { get; set; } = new();
}
