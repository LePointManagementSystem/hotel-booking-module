using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain;

namespace HotelBookingPlatform.Domain.Abstracts
{
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        Task<IEnumerable<Staff>> GetActiveStaffAsync();
        Task<Staff?> GetByUserIdAsync(string userId);
    }
}