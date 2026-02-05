using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.Abstracts;

public interface ICashSessionRepository
{
    Task<CashSession?> GetOpenSessionAsync(int hotelId, CurrencyCode currency, CashShift shift);
    Task<CashSession?> GetByIdAsync(int id);
    Task CreateAsync(CashSession session);
    Task UpdateAsync(CashSession session);

    Task<IReadOnlyList<CashSession>> GetByHotelAsync(
        int hotelId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CurrencyCode? currency,
        CashShift? shift,
        int page,
        int pageSize
    );
}