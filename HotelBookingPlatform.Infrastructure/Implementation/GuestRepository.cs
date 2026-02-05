using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Implementation
{
    public class GuestRepository : IGuestRepository
    {
        private readonly AppDbContext _db;

        public GuestRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Guest?> GetByCinAsync(string cin)
        {
            var normalized = cin.Trim();
            return await _db.Guests.FirstOrDefaultAsync(g => g.CIN == normalized);
        }

        public async Task<Guest?> GetByIdAsync(Guid id)
        {
            return await _db.Guests.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Guest> CreateAsync(Guest guest)
        {
            _db.Guests.Add(guest);

            return guest;
        }
    }
}