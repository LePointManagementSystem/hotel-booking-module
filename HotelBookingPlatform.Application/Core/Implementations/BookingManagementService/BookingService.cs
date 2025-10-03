using System.Runtime.CompilerServices;
using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
namespace HotelBookingPlatform.Application.Core.Implementations.BookingManagementService;

public class BookingService : BaseService<Booking>, IBookingService
{
    private readonly IConfirmationNumberGeneratorService _confirmationNumberGeneratorService;
    private readonly IPriceCalculationService _priceCalculationService;
    public BookingService(
        IUnitOfWork<Booking> unitOfWork,
        IMapper mapper,
        IConfirmationNumberGeneratorService confirmationNumberGeneratorService,
        IPriceCalculationService priceCalculationService)
        : base(unitOfWork, mapper)
    {
        _confirmationNumberGeneratorService = confirmationNumberGeneratorService;
        _priceCalculationService = priceCalculationService;
    }

    public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _unitOfWork.BookingRepository.GetAllBookingsAsync();
        var result = bookings.Select(b =>
        {
            var dto = _mapper.Map<BookingDto>(b);
            dto.DurationType = b.DurationType.ToString();
            dto.UserName = b.User?.UserName;
            return dto;
        });
        return result;
    }
    public async Task<BookingDto> GetBookingAsync(int id)
    {
        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(id);

        if (booking is null)
            throw new NotFoundException($"Booking with ID {id} not found.");

        var user = await _unitOfWork.UserRepository.GetByIdAsync(booking.UserId);
        var bookingDto = _mapper.Map<BookingDto>(booking);
        bookingDto.UserName = user?.UserName;

        return bookingDto;
    }
    public async Task<BookingDto> CreateBookingAsync(BookingCreateRequest request, string email)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user is null)
            throw new NotFoundException("User not found.");

        var checkOut = request.DurationType switch
        {
            BookingDurationType.Hours2 => request.CheckInDateUtc.AddHours(2),
            BookingDurationType.Hours4 => request.CheckInDateUtc.AddHours(4),
            BookingDurationType.Overnight => request.CheckInDateUtc.AddHours(24),
            _ => throw new ArgumentOutOfRangeException()
        };

        var totalPrice = await _priceCalculationService.CalculateTotalPriceAsync(
            request.RoomIds.ToList(),
            request.CheckInDateUtc,
            checkOut
        );

        var discountedTotalPrice = await _priceCalculationService.CalculateDiscountedPriceAsync(
            request.RoomIds.ToList(),
            request.CheckInDateUtc,
            checkOut
        );

        var booking = new Booking
        {
            UserId = user.Id,
            User = user,
            ConfirmationNumber = _confirmationNumberGeneratorService.GenerateConfirmationNumber(),
            TotalPrice = totalPrice,
            AfterDiscountedPrice = discountedTotalPrice,
            BookingDateUtc = DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod,
            Hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId),
            CheckInDateUtc = request.CheckInDateUtc,
            CheckOutDateUtc = checkOut,
            DurationType = request.DurationType,
            Status = BookingStatus.Pending,
            Rooms = new List<Room>()
        };

        foreach (var roomId in request.RoomIds)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room is not null)
            {
                room.IsAvailable = false;
                booking.Rooms.Add(room);
                await _unitOfWork.RoomRepository.UpdateAsync(room.RoomID, room);
            }
                
        }

        await _unitOfWork.BookingRepository.CreateAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<BookingDto>(booking);
        result.DurationType = request.DurationType.ToString();
        return result;
    }
    public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus)
    {
        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);

        if (booking is null)
            throw new NotFoundException($"Booking with ID {bookingId} not found.");

        if (booking.Status == BookingStatus.Completed && newStatus != BookingStatus.Completed)
            throw new InvalidOperationException("Cannot change the status of a completed booking.");

        await _unitOfWork.BookingRepository.UpdateBookingStatusAsync(bookingId, newStatus);
        await _unitOfWork.SaveChangesAsync();
    }

  public async Task<List<object>> ReleaseExpiredBookingsAsync()
{
    var now = DateTime.UtcNow;
    var expiredBookings = await _unitOfWork.BookingRepository.GetExpiredBookingsWithRoomsAsync(now);
    var releasedData = new List<object>();

    foreach (var booking in expiredBookings)
    {
        booking.Status = BookingStatus.Completed;
        await _unitOfWork.BookingRepository.UpdateAsync(booking.BookingID, booking); // <-- ✅ important

        foreach (var room in booking.Rooms)
        {
            room.IsAvailable = true;

            // ✅ Marque explicitement la chambre comme modifiée
            await _unitOfWork.RoomRepository.UpdateAsync(room.RoomID, room);

            releasedData.Add(new
            {
                BookingId = booking.BookingID,
                RoomId = room.RoomID,
                RoomNumber = room.Number,
                BookingExpiredAt = booking.CheckOutDateUtc,
                StatusUpdateTo = "Completed"
            });
        }
    }

    await _unitOfWork.SaveChangesAsync();
    return releasedData;
}




}

