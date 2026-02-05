using HotelBookingPlatform.Domain.Entities;

namespace HotelBookingPlatform.Domain.Abstracts
{
    public interface IGuestRepository
    {
        
        Task<Guest?> GetByCinAsync(string cin);
        Task<Guest?> GetByIdAsync(Guid id);
        Task<Guest> CreateAsync(Guest guest);
    }
}