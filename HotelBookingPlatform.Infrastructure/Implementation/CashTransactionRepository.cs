using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Implementation;

public class CashTransactionRepository : GenericRepository<CashTransaction>, ICashTransactionRepository
{
    public CashTransactionRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CashTransaction>> GetByHotelAsync(
        int hotelId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CashTransactionType? type = null,
        CurrencyCode? currency = null,
        int page = 1,
        int pageSize = 100)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 500 ? 100 : pageSize;

        var q = _appDbContext.Set<CashTransaction>()
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Where(x => x.HotelId == hotelId);

        if (fromUtc.HasValue) q = q.Where(x => x.CreatedAtUtc >= fromUtc.Value);
        if (toUtc.HasValue) q = q.Where(x => x.CreatedAtUtc <= toUtc.Value);
        if (type.HasValue) q = q.Where(x => x.Type == type.Value);
        if (currency.HasValue) q = q.Where(x => x.Currency == currency.Value);

        return await q
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
