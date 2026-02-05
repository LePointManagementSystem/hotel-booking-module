using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Implementation;

public class CashSessionRepository : ICashSessionRepository
{
    private readonly AppDbContext _db;

    public CashSessionRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<CashSession?> GetByIdAsync(int id)
    {
        return _db.CashSessions
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.CashSessionId == id);

    }

    public Task<CashSession?>GetOpenSessionAsync(int hotelId, CurrencyCode currency, CashShift shift)
    {
        return _db.CashSessions
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.HotelId == hotelId && x.Currency == currency && x.Shift == shift && x.ClosedAtUtc == null);
    }

    public async Task CreateAsync(CashSession session)
    {
        await _db.CashSessions.AddAsync(session);
    } 

    public Task UpdateAsync(CashSession session)
    {
        _db.CashSessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<CashSession>> GetByHotelAsync(
        int hotelId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CurrencyCode? currency,
        CashShift? shift,
        int page,
        int pageSize
    )

    {
        var q = _db.CashSessions
            .Include(x => x.Transactions)
            .Where(x => x.HotelId == hotelId)
            .AsQueryable();

        if(fromUtc.HasValue) q = q.Where(x => x.OpenedAtUtc >= fromUtc.Value);
        if(toUtc.HasValue) q = q.Where(x => x.OpenedAtUtc <= toUtc.Value);
        if(currency.HasValue) q = q.Where(x => x.Currency == currency.Value);
        if(shift.HasValue) q = q.Where(x => x.Shift == shift.Value);

        if(page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 50;

        return await q.OrderByDescending(x => x.OpenedAtUtc)
            .Skip((page -1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    }
}