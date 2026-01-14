using HotelBookingPlatform.Application.Core.Abstracts.ReportsManagementService;
using HotelBookingPlatform.Domain.DTOs.Reports;
using HotelBookingPlatform.Domain.Enums;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Application.Core.Implementations.ReportsManagementService;

public sealed class ReportsService : IReportsService
{
    private readonly AppDbContext _db;

    public ReportsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MonthlyHotelReportDto> GetMonthlyHotelReportAsync(int hotelId, int year, int month)
    {
        if (year < 2000 || year > 2100) throw new ArgumentOutOfRangeException(nameof(year));
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));

        var periodStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEndUtc = periodStartUtc.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        // Total rooms for the hotel (Room -> RoomClass -> HotelId)
        var totalRooms = await _db.RoomClasses
            .AsNoTracking()
            .Where(rc => rc.HotelId == hotelId)
            .SelectMany(rc => rc.Rooms)
            .CountAsync();

        // Bookings created in the period (KPI + revenue)
        var bookingsCreated = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.HotelId == hotelId && b.BookingDateUtc >= periodStartUtc && b.BookingDateUtc < periodEndUtc)
            .Select(b => new
            {
                b.Status,
                b.TotalPrice,
                b.CancellationReason
            })
            .ToListAsync();

        var bookingsCreatedCount = bookingsCreated.Count;
        var confirmedCount = bookingsCreated.Count(b => b.Status == BookingStatus.Confirmed);
        var completedCount = bookingsCreated.Count(b => b.Status == BookingStatus.Completed);
        var cancelledCount = bookingsCreated.Count(b => b.Status == BookingStatus.Cancelled);

        var revenueTotal = bookingsCreated
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Sum(b => b.TotalPrice);

        var revenueCompleted = bookingsCreated
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.TotalPrice);

        // Occupancy: bookings that overlap the period, excluding cancelled.
        // We compute overlap duration and multiply by rooms count.
        var overlappingBookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.HotelId == hotelId &&
                        b.Status != BookingStatus.Cancelled &&
                        b.CheckInDateUtc < periodEndUtc &&
                        b.CheckOutDateUtc > periodStartUtc)
            .Select(b => new
            {
                b.CheckInDateUtc,
                b.CheckOutDateUtc,
                b.DurationType,
                RoomsCount = b.Rooms.Count
            })
            .ToListAsync();

        decimal occupiedRoomNights = 0m;

        foreach (var b in overlappingBookings)
        {
            if (b.RoomsCount <= 0) continue;

            var overlapStart = b.CheckInDateUtc > periodStartUtc ? b.CheckInDateUtc : periodStartUtc;
            var overlapEnd = b.CheckOutDateUtc < periodEndUtc ? b.CheckOutDateUtc : periodEndUtc;

            if (overlapEnd <= overlapStart) continue;

            var overlap = overlapEnd - overlapStart;
            decimal days;


switch (b.DurationType)
{
    case BookingDurationType.Hours2:
        // 2 hours = 2/24 day per booked block
        // But overlap might be less/more depending on date ranges; keep it overlap-driven:
        days = (decimal)overlap.TotalHours / 24m;
        break;

    case BookingDurationType.Hours4:
        // 4 hours = 4/24 day per booked block
        // Same overlap-driven approach:
        days = (decimal)overlap.TotalHours / 24m;
        break;

    case BookingDurationType.Overnight:
    default:
        days = (decimal)overlap.TotalDays;
        break;
}

        }

        var availableRoomNights = (decimal)totalRooms * daysInMonth;
        var occupancyRate = availableRoomNights <= 0 ? 0m : occupiedRoomNights / availableRoomNights;

        // Cash summary
        var cashInMonth = await _db.CashTransactions
            .AsNoTracking()
            .Where(c => c.HotelId == hotelId && c.CreatedAtUtc >= periodStartUtc && c.CreatedAtUtc < periodEndUtc)
            .Select(c => new { c.Currency, c.Type, c.Amount })
            .ToListAsync();

        var cashSummary = cashInMonth
            .GroupBy(c => c.Currency)
            .Select(g => new CashMonthlySummaryDto
            {
                Currency = g.Key,
                TotalIn = g.Where(x => x.Type == CashTransactionType.In).Sum(x => x.Amount),
                TotalOut = g.Where(x => x.Type == CashTransactionType.Out).Sum(x => x.Amount)
            })
            .OrderBy(x => x.Currency)
            .ToList();

        // Top cancellation reasons
        var topReasons = bookingsCreated
            .Where(b => b.Status == BookingStatus.Cancelled)
            .Select(b => (b.CancellationReason ?? string.Empty).Trim())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .GroupBy(r => r)
            .Select(g => new CancellationReasonCountDto { Reason = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Reason)
            .Take(10)
            .ToList();

        return new MonthlyHotelReportDto
        {
            HotelId = hotelId,
            Year = year,
            Month = month,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc,
            DaysInMonth = daysInMonth,
            TotalRooms = totalRooms,
            OccupiedRoomNights = decimal.Round(occupiedRoomNights, 4),
            AvailableRoomNights = availableRoomNights,
            OccupancyRate = decimal.Round(occupancyRate, 6),
            BookingsCreatedCount = bookingsCreatedCount,
            ConfirmedCount = confirmedCount,
            CompletedCount = completedCount,
            CancelledCount = cancelledCount,
            RevenueTotal = revenueTotal,
            RevenueCompleted = revenueCompleted,
            CashSummary = cashSummary,
            TopCancellationReasons = topReasons
        };
    }
}
