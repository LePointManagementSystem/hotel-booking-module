namespace HotelBookingPlatform.Domain.Abstracts
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        public Task<Room> GetByIdAsync(int id);
        //Task UpdateRangeAsync(IEnumerable<Room> rooms);

        Task<IEnumerable<Room>> GetRoomsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Room>> GetAvailableRoomsWithNoBookingsAsync(int roomClassId);
        Task<IEnumerable<Room>> GetRoomsAvailableBetweenDatesAsync(int roomClassId, DateTime checkIn, DateTime checkOut);
        Task<bool> HasBookingConflictAsync(int roomId, DateTime checkInUtc, DateTime checkOutUtc, int? ignoreBookingId = null);
    }
}