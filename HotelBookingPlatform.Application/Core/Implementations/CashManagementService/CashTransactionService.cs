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

    public async Task<CashTransactionDto> CreateAsync(CreateCashTransactionRequest request, string actorUserId, int? scopedHotelId, bool isStaff)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(actorUserId)) throw new ArgumentException("actorUserId is required", nameof(actorUserId));

        // Staff users are scoped to one hotel.
        var hotelId = isStaff ? (scopedHotelId ?? 0) : request.HotelId;
        if (hotelId <= 0) throw new InvalidOperationException("HotelId is required.");
        if (isStaff && scopedHotelId.HasValue && request.HotelId != 0 && request.HotelId != scopedHotelId.Value)
            throw new UnauthorizedAccessException("You are not allowed to create cash transactions for another hotel.");

        if (request.Amount <= 0) throw new InvalidOperationException("Amount must be greater than 0.");

        var note = (request.Note ?? string.Empty).Trim();
        if (request.Type == CashTransactionType.Out && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("Note is required for cash OUT transactions.");

        var entity = new CashTransaction
        {
            HotelId = hotelId,
            ActorUserId = actorUserId,
            Type = request.Type,
            Currency = request.Currency,
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
        int page,
        int pageSize,
        int? scopedHotelId,
        bool isStaff)
    {
        // Staff users are scoped to one hotel.
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
            Amount = x.Amount,
            Note = x.Note,
            Category = x.Category,
            Reference = x.Reference,
            CreatedAtUtc = x.CreatedAtUtc
        };
    }
}
