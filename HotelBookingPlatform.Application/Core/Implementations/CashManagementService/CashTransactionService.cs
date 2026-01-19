using HotelBookingPlatform.Application.Core.Abstracts.CashManagementService;
using HotelBookingPlatform.Domain;
using HotelBookingPlatform.Domain.DTOs.Cash;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.Core.Implementations.CashManagementService;

public class CashTransactionService : ICashTransactionService
{
    private readonly IUnitOfWork<CashTransaction> _unitOfWork;

    public CashTransactionService(IUnitOfWork<CashTransaction> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CashTransactionDto> CreateAsync(
        CreateCashTransactionRequest request,
        string actorUserId,
        int? scopedHotelId,
        bool isStaff)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(actorUserId)) throw new ArgumentException("actorUserId is required", nameof(actorUserId));
        if (request.Amount <= 0) throw new InvalidOperationException("Amount must be greater than 0.");

        var note = (request.Note ?? string.Empty).Trim();
        if (request.Type == CashTransactionType.Out && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("Note is required for cash OUT transactions.");

        // Phase 2: require an open session
        if (!request.CashSessionId.HasValue || request.CashSessionId.Value <= 0)
            throw new InvalidOperationException("CashSessionId is required. Open a shift first.");

        var session = await _unitOfWork.CashSessionRepository.GetByIdAsync(request.CashSessionId.Value);
        if (session == null) throw new InvalidOperationException("Cash session not found.");
        if (session.IsClosed) throw new InvalidOperationException("This shift is closed. Open a new shift.");

        // Enforce staff scope by hotelId from token
        if (isStaff)
        {
            if (!scopedHotelId.HasValue) throw new UnauthorizedAccessException("Staff hotel scope is missing.");
            if (session.HotelId != scopedHotelId.Value)
                throw new UnauthorizedAccessException("You cannot create transactions for another hotel.");
        }

        var entity = new CashTransaction
        {
            HotelId = session.HotelId,
            CashSessionId = session.CashSessionId,
            ActorUserId = actorUserId,
            Type = request.Type,
            Currency = session.Currency,
            Shift = session.Shift,
            Amount = request.Amount,
            Note = note,
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.CashTransactionRepository.CreateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(entity);
    }

    public async Task<IReadOnlyList<CashTransactionDto>> GetByHotelAsync(
        int hotelId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CashTransactionType? type,
        CurrencyCode? currency,
        CashShift? shift,
        int page,
        int pageSize,
        int? scopedHotelId,
        bool isStaff)
    {
        if (isStaff)
        {
            if (!scopedHotelId.HasValue) throw new UnauthorizedAccessException("Staff hotel scope is missing.");
            if (hotelId != 0 && hotelId != scopedHotelId.Value)
                throw new UnauthorizedAccessException("You are not allowed to access cash transactions from another hotel.");
            hotelId = scopedHotelId.Value;
        }

        if (hotelId <= 0) throw new InvalidOperationException("HotelId is required.");

        var items = await _unitOfWork.CashTransactionRepository.GetByHotelAsync(
            hotelId,
            fromUtc,
            toUtc,
            type,
            currency,
            shift,
            page,
            pageSize);

        return items.Select(ToDto).ToList();
    }

    private static CashTransactionDto ToDto(CashTransaction x)
    {
        return new CashTransactionDto
        {
            CashTransactionId = x.CashTransactionID,
            HotelId = x.HotelId,
            ActorUserId = x.ActorUserId,
            Type = x.Type,
            Currency = x.Currency,
            Shift = x.Shift,
            Amount = x.Amount,
            Note = x.Note,
            Category = x.Category,
            Reference = x.Reference,
            CreatedAtUtc = x.CreatedAtUtc
        };
    }
}
