using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Infrastructure.Data;
using HotelBookingPlatform.Infrastructure.Implementation;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Implementation
{
    public class StaffRepository : GenericRepository<Staff>, IStaffRepository
    {
        public StaffRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Staff>> GetActiveStaffAsync()
        {
            return await _appDbContext.Set<Staff>()
                .Include(s => s.Hotel)
                .Where(s => s.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }
        
        public  async Task<Staff> GetByIdAsync(int id)
        {
            var entity = await _appDbContext.Set<Staff>()
                .Include(s => s.Hotel)
                .FirstOrDefaultAsync(s => s.StaffId == id);

            if (entity == null)
                throw new KeyNotFoundException($"Staff with ID {id} not found.");
            return entity;
        }

        public async Task<Staff?> GetByUserIdAsync(string userId)
        {
            return await _appDbContext.Set<Staff>()
                .Include(s => s.Hotel)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<IEnumerable<Staff>> GetAllWithHotelAsync()
    {
        return await _appDbContext.Set<Staff>()
            .Include(s => s.Hotel)
            .AsNoTracking()
            .ToListAsync();
    }

    }
}