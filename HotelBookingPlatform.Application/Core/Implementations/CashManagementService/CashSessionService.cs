using HotelBookingPlatform.Application.Core.Abstracts.CashManagementService;
using HotelBookingPlatform.Domain;
using HotelBookingPlatform.Domain.DTOs.Cash;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.Core.Implementations.CashManagementService;

public class CashSessionService : ICashSessionService
{
    private readonly IUnitOfWork<CashSession> _unitOfWork;

    public CashSessionService(IUnitOfWork<CashSession> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CashSessionDto> OpenAsync(OpenCashSessionRequest request, string actorUserId, int? scopedHotelId, bool isStaff)
    {
        if(request.OpeningBalance <0 ) throw new InvalidOperationException("Opening balance must be >= 0.");
        if(string.IsNullOrWhiteSpace(actorUserId)) throw new InvalidOperationException("actorUserId is required.");

        var hotelId = isStaff ? (scopedHotelId ?? 0) : request.HotelId;
        if (hotelId <= 0) throw new InvalidOperationException("HotelId is required.");

        var existing = await _unitOfWork.CashSessionRepository.GetOpenSessionAsync(hotelId, request.Currency, request.Shift);
        if (existing != null)
            throw new InvalidOperationException("A shift is already open for this hotel/currency/shift. Close it first.");
        
        var session = new CashSession
        {
            HotelId = hotelId,
            OpenedByUserId = actorUserId,
            Currency = request.Currency,
            Shift = request.Shift,
            OpeningBalance = request.OpeningBalance,
            OpenedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.CashSessionRepository.CreateAsync(session);
        await _unitOfWork.SaveChangesAsync();
        
        return ToDto(session);
    }

    public async Task<CashSessionDto> CloseAsync(int cashSessionId, CloseCashSessionRequest request, string actorUserId, int? scopedHotelId, bool isStaff)
    {
        if (request.ClosingCounted < 0) throw new InvalidOperationException("Closing counted must be >= 0.");

        var session = await _unitOfWork.CashSessionRepository.GetByIdAsync(cashSessionId);
        if(session == null ) throw new KeyNotFoundException("Cash session not found.");
        if(session.IsClosed) throw new InvalidOperationException("Cash session is already closed.");

        if (isStaff)
        {
            if (!scopedHotelId.HasValue) throw new UnauthorizedAccessException("Staff hotel scope is missing.");
            if (session.HotelId != scopedHotelId.Value)
                throw new UnauthorizedAccessException("You cannot close a shift for another hotel.");
            
        }

        session.ClosingCounted = request.ClosingCounted;
        session.ClosedByUserId = actorUserId;
        session.ClosedAtUtc = DateTime.UtcNow;

        await _unitOfWork.CashSessionRepository.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(session);
    }

    public async Task<IReadOnlyList<CashSessionDto>> GetByHotelAsync(
        int hotelId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CurrencyCode? currency,
        CashShift? shift,
        int page,
        int pageSize,
        int? scopedHotelId,
        bool isStaff
    )
    {
        if(isStaff)
        {
            if(!scopedHotelId.HasValue) throw new UnauthorizedAccessException("Staff hotel scope is missing.");
            if(hotelId != 0 && hotelId != scopedHotelId.Value)
                throw new UnauthorizedAccessException("You cannot access another hotel.");
            hotelId = scopedHotelId.Value;

        }

        if (hotelId <= 0) throw new InvalidOperationException("HotelId is required.");

        var list = await _unitOfWork.CashSessionRepository.GetByHotelAsync(hotelId, fromUtc, toUtc, currency, shift,page, pageSize);

        return list.Select(ToDto).ToList();
    }

    private static CashSessionDto ToDto(CashSession s)
    {
        var expected = s.OpeningBalance;
        if(s.Transactions != null && s.Transactions.Count > 0)
        {
            var totalIn = s.Transactions.Where(t => t.Type == CashTransactionType.In).Sum(t => t.Amount);
            var totalOut = s.Transactions.Where(t => t.Type == CashTransactionType.Out).Sum(t => t.Amount);
            expected = s.OpeningBalance + totalIn - totalOut;
        }

        var counted = s.ClosingCounted ?? expected;
        var diff = counted - expected;

        return new CashSessionDto
        {
            CashSessionId = s.CashSessionId,
            HotelId = s.HotelId,
            Currency = s.Currency,
            Shift = s.Shift,
            OpeningBalance = s.OpeningBalance,
            OpenedAtUtc = s.OpenedAtUtc,
            ClosingCounted = s.ClosingCounted,
            ClosedAtUtc = s.ClosedAtUtc,
            Expected = expected,
            Difference = diff,
            IsClosed = s.IsClosed
        };

    }
        
}