namespace HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<BookingDto> GetBookingAsync(int id);
    Task<BookingDto> CreateBookingAsync(BookingCreateRequest request, string userId);
    Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus);
    Task<List<object>> ReleaseExpiredBookingsAsync();

}