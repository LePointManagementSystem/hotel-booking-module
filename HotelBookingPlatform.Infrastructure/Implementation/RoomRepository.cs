using System.ComponentModel.Design;
using System.Threading.Tasks.Dataflow;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace HotelBookingPlatform.Infrastructure.Implementation
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(AppDbContext context)
            : base(context) { }

        public async Task<IEnumerable<Room>> GetRoomsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _appDbContext.Rooms
                .Include(rc => rc.RoomClass)
                .Where(r => r.PricePerNight >= minPrice && r.PricePerNight <= maxPrice)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsWithNoBookingsAsync(int roomClassId)
        {
            var now = DateTime.UtcNow;

            var bookedRoomIds = await _appDbContext.Bookings
                .Where(b =>
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                b.CheckInDateUtc <= now &&
                b.CheckOutDateUtc >= now)
                .SelectMany(b => b.Rooms.Select(r => r.RoomID))
                .Distinct()
                .ToListAsync();

            var availableRooms = await _appDbContext.Rooms
                .Include(r => r.RoomClass)
                .Where(r =>
                r.RoomClassID == roomClassId &&
                r.IsAvailable &&
                !bookedRoomIds.Contains(r.RoomID)
                )
                .ToListAsync();

            return availableRooms;
                
        }

        public async Task<Room> GetByIdAsync(int id)
        {
            return await _appDbContext.Rooms
                .Include(r => r.RoomClass)
                .FirstOrDefaultAsync(r => r.RoomID == id);
        }

        public async Task<IEnumerable<Room>> GetRoomsAvailableBetweenDatesAsync(int roomClassId, DateTime checkIn, DateTime checkOut)
        {
            // Étape 1 : trouver les IDs des chambres réservées pendant la période
            var bookedRoomIds = await _appDbContext.Bookings
                .Where(b =>
                    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                    b.Rooms.Any() &&
                    !(checkOut <= b.CheckInDateUtc || checkIn >= b.CheckOutDateUtc)
                )
                .SelectMany(b => b.Rooms.Select(r => r.RoomID))
                .Distinct()
                .ToListAsync();

            // Étape 2 : retourner les chambres disponibles (hors des ID réservés)
            return await _appDbContext.Rooms
                .Include(r => r.RoomClass)
                .Where(r =>
                    r.RoomClassID == roomClassId &&
                    r.IsAvailable && // Optionnel : si tu gères aussi manuellement la dispo
                    !bookedRoomIds.Contains(r.RoomID)
                )
                .ToListAsync();
        }
    }
}
