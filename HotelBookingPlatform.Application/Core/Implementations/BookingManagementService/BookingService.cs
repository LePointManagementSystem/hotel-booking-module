using System.Runtime.CompilerServices;
using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
using HotelBookingPlatform.Application.Core.Abstracts.NotificationManagementService;
using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;
namespace HotelBookingPlatform.Application.Core.Implementations.BookingManagementService;

public class BookingService : BaseService<Booking>, IBookingService
{
    private readonly IConfirmationNumberGeneratorService _confirmationNumberGeneratorService;
    private readonly UserManager<LocalUser> _userManager;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly INotificationService _notificationService;
    public BookingService(
        IUnitOfWork<Booking> unitOfWork,
        IMapper mapper,
        IConfirmationNumberGeneratorService confirmationNumberGeneratorService,
        IPriceCalculationService priceCalculationService,
        UserManager<LocalUser> userManager,
        INotificationService notificationService)
        : base(unitOfWork, mapper)
    {
        _confirmationNumberGeneratorService = confirmationNumberGeneratorService;
        _priceCalculationService = priceCalculationService;
        _userManager = userManager;
        _notificationService = notificationService;
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


        // Normalize check-in / check-out based on duration type.
        // NOTE: Frontend sends an enum number (see BookingDurationType). We support:
        // - Hours1..Hours8  => checkOut = checkIn + N hours
        // - Hours2 / Hours4 => same behavior as before
        // - Overnight       => uses hotel overnight range helper
        // - Stay            => uses request.CheckOutDateUtc (multi-day)

        var (checkInUtc, checkOutUtc) = request.DurationType switch
        {
            BookingDurationType.Overnight => CalculateOvernightRange(request.CheckInDateUtc),

            BookingDurationType.Stay => (request.CheckInDateUtc, request.CheckOutDateUtc),

            _ => (request.CheckInDateUtc, request.CheckInDateUtc.AddHours(GetDurationHours(request.DurationType)))
        };

        if (checkOutUtc <= checkInUtc)
            throw new InvalidOperationException("Check-out must be after check-in.");

        // For Stay, enforce at least 1 night (24h) to avoid accidental hourly payloads.
        if (request.DurationType == BookingDurationType.Stay)
        {
            var totalHours = (checkOutUtc - checkInUtc).TotalHours;
            if (totalHours < 24)
                throw new InvalidOperationException("Stay duration must be at least 24 hours.");
        }

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

        // 🔔 Notification: booking created/confirmed (hotel scoped)
        try
        {
            var roomNumbers = booking.Rooms?.Select(r => $"#{r.Number}").ToList() ?? new List<string>();
            var roomsText = roomNumbers.Count > 0 ? string.Join(", ", roomNumbers) : "(rooms)";

            var title = "Booking Confirmed";
            var message = $"Booking {booking.ConfirmationNumber} for {guest.FirstName} {guest.LastName} - Rooms {roomsText}. " +
                          $"Check-in: {checkInUtc:u}, Check-out: {checkOutUtc:u} ({request.DurationType}).";

            // We keep EventAtUtc as the booking start time (useful for sorting/"in X minutes" on UI)
            await _notificationService.CreateHotelNotificationAsync(
                hotelId: booking.HotelId,
                type: NotificationType.BookingConfirmed,
                title: title,
                message: message,
                eventAtUtc: checkInUtc,
                actorUserId: user.Id,
                recipientUserId: null,
                bookingId: booking.BookingID);
        }
        catch
        {
            // Notification must not break booking creation.
        }

        var result = _mapper.Map<BookingDto>(booking);
        result.DurationType = request.DurationType.ToString();
        result.UserName = user.UserName;
        return result;
    }
   

   public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus)
{
    // ✅ IMPORTANT: load booking WITH Rooms (and optionally Guest/User)
    var booking = await _unitOfWork.BookingRepository.GetByIdWithRoomsAsync(bookingId);
    // Si tu n'as pas cette méthode, je te dis juste après comment la créer.

    if (booking is null)
        throw new NotFoundException($"Booking with ID {bookingId} not found.");

    if (booking.Status == BookingStatus.Completed && newStatus != BookingStatus.Completed)
        throw new InvalidOperationException("Cannot change the status of a completed booking.");

    var previousStatus = booking.Status;

    // ✅ Update status
    booking.Status = newStatus;

    // ✅ Update room availability
    if (booking.Rooms != null && booking.Rooms.Any())
    {
        if (newStatus == BookingStatus.Cancelled || newStatus == BookingStatus.Completed)
        {
            foreach (var room in booking.Rooms)
            {
                room.IsAvailable = true;
                // ✅ no UpdateAsync here (let EF track changes)
            }
        }
        else if (newStatus == BookingStatus.Confirmed)
        {
            foreach (var room in booking.Rooms)
            {
                room.IsAvailable = false;
            }
        }
    }

    // ✅ Create notification when manually completed
    if (previousStatus != BookingStatus.Completed &&
        previousStatus != BookingStatus.Cancelled &&
        newStatus == BookingStatus.Completed)
    {
        var rooms = booking.Rooms?.Select(r => $"#{r.Number}").ToList() ?? new List<string>();
        var roomsLabel = rooms.Count > 0 ? string.Join(", ", rooms) : "-";
        var nowUtc = DateTime.UtcNow;

        await _notificationService.CreateHotelNotificationAsync(
            hotelId: booking.HotelId,
            type: NotificationType.BookingCompleted,
            title: "Booking Completed",
            message: $"Booking {booking.BookingID} completed. Rooms {roomsLabel}.",
            eventAtUtc: nowUtc,
            bookingId: booking.BookingID,
            roomId: null
        );
    }

    // ✅ single commit
    await _unitOfWork.SaveChangesAsync();
}



public async Task CancelBookingAsync(int bookingId, string reason, string? cancelledByUserId)
	{
	    if (string.IsNullOrWhiteSpace(reason))
	        throw new BadRequestException("Cancellation reason is required.");

	    var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);
	    if (booking is null)
	        throw new NotFoundException($"Booking with ID {bookingId} not found.");

	    // If already done, do nothing / avoid changing history
	    if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
	        return;

	    booking.Status = BookingStatus.Cancelled;
	    booking.CancellationReason = reason.Trim();
	    booking.CancelledByUserId = cancelledByUserId;
	    booking.CancelledAtUtc = DateTime.UtcNow;

	    // release rooms
	    if (booking.Rooms != null && booking.Rooms.Any())
	    {
	        foreach (var room in booking.Rooms)
	        {
	            room.IsAvailable = true;
	            await _unitOfWork.RoomRepository.UpdateAsync(room.RoomID, room);
	        }
	    }

	    await _unitOfWork.BookingRepository.UpdateAsync(booking.BookingID, booking);
	    await _unitOfWork.SaveChangesAsync();

        // Create notification for cancellation (do not block the cancellation flow if this fails)
        try
        {
            var roomNumbers = booking.Rooms != null && booking.Rooms.Any()
                ? string.Join(", ", booking.Rooms.Select(r => r.Number))
                : "N/A";

            var title = "Booking cancelled";
            var message = 
                $"Booking #{booking.BookingID} was cancelled." +
                $" Rooms: {roomNumbers}." +
                $" Reason: {reason.Trim()}";

            await _notificationService.CreateHotelNotificationAsync(
                hotelId: booking.HotelId,
                type: NotificationType.BookingCancelled,
                title: title,
                message: message,
                eventAtUtc: DateTime.UtcNow,
                actorUserId: cancelledByUserId ?? "system",
                recipientUserId: null,
                bookingId: booking.BookingID,
                roomId: null
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create cancellation notification for booking {booking.BookingID}: {ex.Message}", "warn");
        }
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

            // 🔔 Notification: booking completed (expired)
            try
            {
                var roomNumbers = booking.Rooms?.Select(r => $"#{r.Number}").ToList() ?? new List<string>();
                var roomsText = roomNumbers.Count > 0 ? string.Join(", ", roomNumbers) : "(rooms)";

                var title = "Booking Completed";
                var message = $"Booking {booking.ConfirmationNumber} completed (time up). Rooms {roomsText}.";

                await _notificationService.CreateHotelNotificationAsync(
                    hotelId: booking.HotelId,
                    type: NotificationType.BookingCompleted,
                    title: title,
                    message: message,
                    eventAtUtc: booking.CheckOutDateUtc,
                    actorUserId: null,
                    recipientUserId: null,
                    bookingId: booking.BookingID);
            }
            catch
            {
                // ignore
            }
        }
    }

    if (releasedData.Any())
    {
        await _unitOfWork.SaveChangesAsync();
    }
    
    return releasedData;
}

    private static int GetDurationHours(BookingDurationType durationType)
    {
        return durationType switch
        {
            BookingDurationType.Hours1 => 1,
            BookingDurationType.Hours2 => 2,
            BookingDurationType.Hours3 => 3,
            BookingDurationType.Hours4 => 4,
            BookingDurationType.Hours5 => 5,
            BookingDurationType.Hours6 => 6,
            BookingDurationType.Hours7 => 7,
            BookingDurationType.Hours8 => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(durationType), durationType, "Unsupported booking duration type.")
        };
    }




}

