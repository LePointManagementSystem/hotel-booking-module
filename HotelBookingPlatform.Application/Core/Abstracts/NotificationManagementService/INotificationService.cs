using HotelBookingPlatform.Domain.DTOs.Notification;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.Core.Abstracts.NotificationManagementService
{
    public interface INotificationService
    {
        // Staff scope (hotel + recipient scope)
        Task<IReadOnlyList<NotificationDto>> GetForCurrentUserAsync(string userId, int hotelId, bool includeRead = true, int page = 1, int pageSize = 50);
        Task<int> GetUnreadCountAsync(string userId, int hotelId);
        Task MarkAsReadAsync(int notificationId, string userId, int hotelId);
        Task MarkAllAsReadAsync(string userId, int hotelId);

        // Admin/Manager scope (no recipient restriction, optional hotel filter)
        Task<IReadOnlyList<NotificationDto>> GetForAdminAsync(int? hotelId, bool includeRead = true, int page = 1, int pageSize = 50);
        Task<int> GetUnreadCountForAdminAsync(int? hotelId);
        Task MarkAsReadAdminAsync(int notificationId);
        Task MarkAllAsReadAdminAsync(int? hotelId);

        Task CreateHotelNotificationAsync(
            int hotelId,
            NotificationType type,
            string title,
            string message,
            DateTime? eventAtUtc = null,
            string? actorUserId = null,
            string? recipientUserId = null,
            int? bookingId = null,
            int? roomId = null);
    }
}
