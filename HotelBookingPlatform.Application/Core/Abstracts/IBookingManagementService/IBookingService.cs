namespace HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<IEnumerable<BookingDto>> GetBookingsByHotelAsync(int hotelId);
    Task<BookingDto> GetBookingAsync(int id);
    Task<BookingDto> CreateBookingAsync(BookingCreateRequest request, string userId);
    Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus);

    // Dedicated cancellation with audit fields.
    // Reason is required by the API endpoint.
    Task CancelBookingAsync(int bookingId, string reason, string? cancelledByUserId);

    Task<List<object>> ReleaseExpiredBookingsAsync();

    
}