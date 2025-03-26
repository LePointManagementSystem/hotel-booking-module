using HotelBookingPlatform.Domain.DTOs.Guest;
using HotelBookingPlatform.Domain.Entities;

namespace HotelBookingPlatform.Application.Core.Abstracts
{
    public interface IGuestService
    {
        Task<GuestResponse> CreateGuestAsync(GuestRequest request);
        Task<IEnumerable<GuestResponse>> GetAllGuestsAsync();
        Task<GuestResponse> GetGuestByIdAsync(Guid id);
        Task<bool> DeleteGuestAsync(Guid id);
    }
}
