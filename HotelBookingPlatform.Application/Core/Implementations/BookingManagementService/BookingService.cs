using System.Runtime.CompilerServices;
using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.Entities;
namespace HotelBookingPlatform.Application.Core.Implementations.BookingManagementService;

public class BookingService : BaseService<Booking>, IBookingService
{
    private readonly IConfirmationNumberGeneratorService _confirmationNumberGeneratorService;
    private readonly UserManager<LocalUser> _userManager;
    private readonly IPriceCalculationService _priceCalculationService;
    public BookingService(
        IUnitOfWork<Booking> unitOfWork,
        IMapper mapper,
        IConfirmationNumberGeneratorService confirmationNumberGeneratorService,
        IPriceCalculationService priceCalculationService,
        UserManager<LocalUser> userManager)
        : base(unitOfWork, mapper)
    {
        _confirmationNumberGeneratorService = confirmationNumberGeneratorService;
        _priceCalculationService = priceCalculationService;
        _userManager = userManager;
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

    public async Task<IEnumerable<BookingDto>> GetBookingsByHotelAsync(int hotelId)
    {
        var bookings = await _unitOfWork.BookingRepository.GetBookingsByHotelAsync(hotelId);
        
        return bookings.Select(b =>
        {
            var dto = _mapper.Map<BookingDto>(b);
            dto.DurationType = b.DurationType.ToString();
            dto.UserName = b.User?.UserName;
            return dto;
        });
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

        // Guest is required for reception flow / confirmation screen
        if (request.Guest == null
            || string.IsNullOrWhiteSpace(request.Guest.FirstName)
            || string.IsNullOrWhiteSpace(request.Guest.LastName)
            || string.IsNullOrWhiteSpace(request.Guest.CIN))
        {
            throw new InvalidOperationException("Guest information is required (FirstName, LastName, CIN).");
        }

        // // recuperer les roles de l'utilisateur
        // var roles = await _userManager.GetRolesAsync(user);
        // var isAdminOrManager = roles.Contains("Admin") || roles.Contains("Manager");

        // var staff = await _unitOfWork.StaffRepository.GetByUserIdAsync(user.Id);
        // int? enforcedHotelId = null;

        // if (!isAdminOrManager && staff !=null && staff.IsActive)
        // {
        //     enforcedHotelId = staff.HotelId;

        //     if (request.HotelId != enforcedHotelId)
        //         throw new InvalidOperationException("You are not allowed to create a booking for another hotel.");
        // }

        var roles = await _userManager.GetRolesAsync(user);
var isAdminOrManager = roles.Contains("Admin") || roles.Contains("Manager");

var staff = await _unitOfWork.StaffRepository.GetByUserIdAsync(user.Id);
int? enforcedHotelId = null;

    if (!isAdminOrManager)
    {
        if (staff == null)
            throw new InvalidOperationException("No staff profile linked to this account.");

        if (!staff.IsActive)
            throw new InvalidOperationException("Your staff account is inactive.");

        enforcedHotelId = staff.HotelId;

        // if (request.HotelId != enforcedHotelId.Value)
        //     throw new InvalidOperationException("You are not allowed to create a booking for another hotel.");
        // Staff ne choisit pas l’hôtel, on force
        request.HotelId = enforcedHotelId.Value;

    }


        var hotelIdToUse = enforcedHotelId ?? request.HotelId;

        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(hotelIdToUse);
        if (hotel is null)
            throw new NotFoundException($"Hotel with ID {hotelIdToUse} not found.");

        // Find or create Guest (by CIN)
        var guestCin = request.Guest.CIN.Trim();
        var existingGuest = await _unitOfWork.GuestRepository.GetByCinAsync(guestCin);
        var guest = existingGuest ?? new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = request.Guest.FirstName.Trim(),
            LastName = request.Guest.LastName.Trim(),
            CIN = guestCin,
            Email = null
        };

        if (existingGuest == null)
        {
            await _unitOfWork.GuestRepository.CreateAsync(guest);
        }


        var (checkInUtc, checkOutUtc) = request.DurationType switch
        {
            BookingDurationType.Hours2 => (request.CheckInDateUtc, request.CheckInDateUtc.AddHours(2)),

            BookingDurationType.Hours4 => (request.CheckInDateUtc, request.CheckInDateUtc.AddHours(4)),

            BookingDurationType.Overnight => CalculateOvernightRange(request.CheckInDateUtc),

            _ => throw new ArgumentOutOfRangeException(nameof(request.DurationType),
            request.DurationType,
            "Unsupported booking duration type.")
        };

        foreach (var roomId in request.RoomIds)
        {
            var hasConflict = await _unitOfWork.RoomRepository
                .HasBookingConflictAsync(roomId, checkInUtc, checkOutUtc);

            if (hasConflict)
            {
                throw new InvalidOperationException($"Room {roomId} is not available for the selected time slot.");
            }
        }

        if (enforcedHotelId.HasValue)
        {
            foreach (var roomId in request.RoomIds)
            {
                var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
                if (room is null)
                    throw new NotFoundException($"Room with ID {roomId} not found.");

                if (room.RoomClass == null)
                    throw new InvalidOperationException("RoomClass not loaded for room.");

                if (room.RoomClass.HotelId != enforcedHotelId.Value)
                {
                    throw new InvalidOperationException($"You are not allowed to book room {room.Number} which belongs to another hotel.");
                }
            }
        }

        var totalPrice = await _priceCalculationService.CalculateTotalPriceAsync(request.RoomIds.ToList(),
        checkInUtc,
        checkOutUtc);

        var discountedTotalPrice = await _priceCalculationService.CalculateDiscountedPriceAsync(request.RoomIds.ToList(),
        checkInUtc,
        checkOutUtc);

        //var hotelIdToUse = enforcedHotelId ?? request.HotelId;
        //var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(hotelIdToUse);

        if (hotel is null)
            throw new NotFoundException($"Hotel with ID {hotelIdToUse} not found.");

        var booking = new Booking
    {
        UserId = user.Id,
        User = user,
        GuestId = guest.Id,
        //Guest = guest,
        ConfirmationNumber = _confirmationNumberGeneratorService.GenerateConfirmationNumber(),
        TotalPrice = totalPrice,
        AfterDiscountedPrice = discountedTotalPrice,
        BookingDateUtc = DateTime.UtcNow,
        PaymentMethod = request.PaymentMethod,

        HotelId = hotelIdToUse, // ✅ IMPORTANT
        //Hotel = hotel,          // ✅ IMPORTANT (déjà chargé au-dessus)

        CheckInDateUtc = checkInUtc,
        CheckOutDateUtc = checkOutUtc,
        DurationType = request.DurationType,
        Status = BookingStatus.Pending,
        Rooms = new List<Room>()
    };


        foreach (var roomId in request.RoomIds)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room is not null)
            {
                booking.Rooms.Add(room);
            }
        }
        
        await _unitOfWork.BookingRepository.CreateAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<BookingDto>(booking);
        result.DurationType = request.DurationType.ToString();
        result.UserName = user.UserName;
        return result;
    }
    public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus)
    {
        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);

        if (booking is null)
            throw new NotFoundException($"Booking with ID {bookingId} not found.");

        if (booking.Status == BookingStatus.Completed && newStatus != BookingStatus.Completed)
            throw new InvalidOperationException("Cannot change the status of a completed booking.");

        // Update of statut
        booking.Status = newStatus;

        //available of room
        if (booking.Rooms != null && booking.Rooms.Any())
        {
            if (newStatus == BookingStatus.Cancelled || newStatus == BookingStatus.Completed)
            {
                foreach (var room in booking.Rooms)
                {
                    room.IsAvailable = true;

                    await _unitOfWork.RoomRepository.UpdateAsync(room.RoomID, room);

                }
            }
            else if (newStatus == BookingStatus.Confirmed)
            {
                foreach (var room in booking.Rooms)
                {
                    room.IsAvailable = false;

                    await _unitOfWork.RoomRepository.UpdateAsync(room.RoomID, room);
                }
            }
        }
        await _unitOfWork.BookingRepository.UpdateAsync(booking.BookingID, booking);
        await _unitOfWork.SaveChangesAsync();
        }


private (DateTime checkInUtc, DateTime checkOutUtc) CalculateOvernightRange(DateTime requestCheckInUtc)
    {
        var local = requestCheckInUtc.ToLocalTime();

        var overnightDate = local.Date;

        var checkInLocal = overnightDate.AddHours(20);

        var checkOutLocal = overnightDate.AddDays(1).AddHours(9);

        return (checkInLocal.ToUniversalTime(),checkOutLocal.ToUniversalTime());
    }
        
    

  public async Task<List<object>> ReleaseExpiredBookingsAsync()
{
    var now = DateTime.UtcNow;
    var expiredBookings = await _unitOfWork.BookingRepository.GetExpiredBookingsWithRoomsAsync(now);
    var releasedData = new List<object>();

    foreach (var booking in expiredBookings)
    {
        // Only process bookings that aren't already completed or cancelled
        if (booking.Status != BookingStatus.Completed && booking.Status != BookingStatus.Cancelled)
        {
            booking.Status = BookingStatus.Completed;
            await _unitOfWork.BookingRepository.UpdateAsync(booking.BookingID, booking);

            if (booking.Rooms != null)
            {
                foreach (var room in booking.Rooms)
                {
                    room.IsAvailable = true;
                    await _unitOfWork.RoomRepository.UpdateAsync(room.RoomID, room);

                    releasedData.Add(new
                    {
                        BookingId = booking.BookingID,
                        RoomId = room.RoomID,
                        RoomNumber = room.Number,
                        BookingExpiredAt = booking.CheckOutDateUtc,
                        StatusUpdateTo = "Completed",
                        LastModifiedUtc = DateTime.UtcNow
                    });
                }
            }
        }
    }

    if (releasedData.Any())
    {
        await _unitOfWork.SaveChangesAsync();
    }
    
    return releasedData;
}




}

