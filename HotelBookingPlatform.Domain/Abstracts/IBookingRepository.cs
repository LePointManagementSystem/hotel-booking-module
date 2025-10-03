namespace HotelBookingPlatform.Domain.Abstracts
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {

        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking> GetByIdAsync(int id);
        Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus);
        Task<Booking> GetBookingByUserAndHotelAsync(string userId, int hotelId);
        Task<IEnumerable<Booking>> GetExpiredBookingsAsync(DateTime now);
        Task<List<Booking>> GetExpiredBookingsWithRoomsAsync(DateTime currentUtcTime);
    }
}