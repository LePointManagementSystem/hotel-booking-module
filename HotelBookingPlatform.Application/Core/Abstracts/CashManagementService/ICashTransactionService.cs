using HotelBookingPlatform.Domain.DTOs.Cash;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.Core.Abstracts.CashManagementService;

public interface ICashTransactionService
{
    Task<CashTransactionDto> CreateAsync(CreateCashTransactionRequest request, string actorUserId, int? scopedHotelId, bool isStaff);

    Task<IReadOnlyList<CashTransactionDto>> GetByHotelAsync(
        int hotelId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CashTransactionType? type,
        CurrencyCode? currency,
        CashShift? shift,
        int page,
        int pageSize,
        int? scopedHotelId,
        bool isStaff);
}
