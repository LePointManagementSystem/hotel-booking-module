using HotelBookingPlatform.Domain.DTOs.Reports;

namespace HotelBookingPlatform.Application.Core.Abstracts.ReportsManagementService;

public interface IReportsService
{
    Task<MonthlyHotelReportDto> GetMonthlyHotelReportAsync(int hotelId, int year, int month);
}
