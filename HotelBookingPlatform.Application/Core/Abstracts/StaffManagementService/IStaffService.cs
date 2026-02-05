using HotelBookingPlatform.Domain.DTOs.Staff;

namespace HotelBookingPlatform.Application.Core.Abstracts.StaffManagementService
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffResponseDto>> GetAllAsync();
        Task<StaffResponseDto> CreateAsync(StaffCreateRequest request);
        Task<StaffResponseDto> UpdateAsync(int id, StaffCreateRequest request);
        Task DeleteAsync(int id);
        Task<StaffResponseDto> UpdateStatusAsync(int id, bool IsActive);

        Task<StaffResponseDto> AttachUserAsync(int StaffId, string userEmail);

        Task<StaffResponseDto> GetByUserIdAsync(string userId);
   

    }
}