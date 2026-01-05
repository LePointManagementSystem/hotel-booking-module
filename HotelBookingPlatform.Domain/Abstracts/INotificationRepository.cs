using HotelBookingPlatform.Domain.Entities;

namespace HotelBookingPlatform.Domain.Abstracts
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> GetForHotelAsync(int hotelId, string? userId, bool includeRead, int skip, int take);
        Task<int> CountUnreadAsync(int hotelId, string? userId);
        Task<Notification?> GetScopedByIdAsync(int notificationId, int hotelId, string? userId);
        // Admin/Manager (no hotel scope, no recipient scope)
        Task<IReadOnlyList<Notification>> GetAllAsync(int? hotelId, bool includeRead, int skip, int take);
        Task<int> CountUnreadAllAsync(int? hotelId);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task MarkAsReadAsync(Notification notification);
        Task MarkAllAsReadAsync(int hotelId, string? userId);
        Task MarkAllAsReadAdminAsync(int? hotelId);
    }
}
