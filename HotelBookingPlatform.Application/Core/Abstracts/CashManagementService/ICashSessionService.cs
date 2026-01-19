using HotelBookingPlatform.Domain.DTOs.Cash;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.Core.Abstracts.CashManagementService;

public interface ICashSessionService
{
    Task<CashSessionDto> OpenAsync(OpenCashSessionRequest request, string actorUserId, int? scopedHotelId, bool isStaff);
    Task<CashSessionDto> CloseAsync(int cashSessionId, CloseCashSessionRequest request, string actorUserId, int? scopedHotelId, bool isStaff);
    Task<IReadOnlyList<CashSessionDto>> GetByHotelAsync(int hotelId, DateTime? fromUtc, DateTime? toUtc, CurrencyCode? currency, CashShift? shift, int page, int pageSize, int? scopedHotelId, bool isStaff);
}