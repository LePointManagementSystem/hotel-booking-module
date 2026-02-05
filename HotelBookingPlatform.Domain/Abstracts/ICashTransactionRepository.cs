using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.Abstracts;

public interface ICashTransactionRepository : IGenericRepository<CashTransaction>
{
    Task<IReadOnlyList<CashTransaction>> GetByHotelAsync(
        int hotelId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CashTransactionType? type = null,
        CurrencyCode? currency = null,
        CashShift? shift = null,
        int page = 1,
        int pageSize = 100);
}
