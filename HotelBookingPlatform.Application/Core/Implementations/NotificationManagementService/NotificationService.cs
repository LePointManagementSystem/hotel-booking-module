using HotelBookingPlatform.Application.Core.Abstracts.NotificationManagementService;
using HotelBookingPlatform.Domain;
using HotelBookingPlatform.Domain.DTOs.Notification;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.Core.Implementations.NotificationManagementService
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork<Notification> _unitOfWork;

    public NotificationService(IUnitOfWork<Notification> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetForCurrentUserAsync(string userId, int hotelId, bool includeRead = true, int page = 1, int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 50;
            if (pageSize > 200) pageSize = 200;

            var skip = (page - 1) * pageSize;
            var notifications = await _unitOfWork.NotificationRepository
                .GetForHotelAsync(hotelId, userId, includeRead, skip, pageSize);

            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                HotelId = n.HotelId,
                Type = n.Type.ToString(),
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc,
                EventAtUtc = n.EventAtUtc,
                ReadAtUtc = n.ReadAtUtc,
                BookingId = n.BookingId,
                RoomId = n.RoomId
            }).ToList();
        }

        public async Task<int> GetUnreadCountAsync(string userId, int hotelId)
        {
            return await _unitOfWork.NotificationRepository.CountUnreadAsync(hotelId, userId);
        }

        public async Task MarkAsReadAsync(int notificationId, string userId, int hotelId)
        {
            var n = await _unitOfWork.NotificationRepository.GetScopedByIdAsync(notificationId, hotelId, userId);
            if (n is null)
                throw new NotFoundException($"Notification with ID {notificationId} not found.");

            await _unitOfWork.NotificationRepository.MarkAsReadAsync(n);
        }

        public async Task MarkAllAsReadAsync(string userId, int hotelId)
        {
            await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(hotelId, userId);
        }

    public async Task<IReadOnlyList<NotificationDto>> GetForAdminAsync(int? hotelId, bool includeRead = true, int page = 1, int pageSize = 50)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var skip = (page - 1) * pageSize;
        var notifications = await _unitOfWork.NotificationRepository
            .GetAllAsync(hotelId, includeRead, skip, pageSize);

        return notifications.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            HotelId = n.HotelId,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAtUtc = n.CreatedAtUtc,
            EventAtUtc = n.EventAtUtc,
            ReadAtUtc = n.ReadAtUtc,
            BookingId = n.BookingId,
            RoomId = n.RoomId
        }).ToList();
    }

    public async Task<int> GetUnreadCountForAdminAsync(int? hotelId)
    {
        return await _unitOfWork.NotificationRepository.CountUnreadAllAsync(hotelId);
    }

    public async Task MarkAsReadAdminAsync(int notificationId)
    {
        var n = await _unitOfWork.NotificationRepository.GetByIdAsync(notificationId);
        if (n is null)
            throw new NotFoundException($"Notification with ID {notificationId} not found.");

        await _unitOfWork.NotificationRepository.MarkAsReadAsync(n);
    }

    public async Task MarkAllAsReadAdminAsync(int? hotelId)
    {
        await _unitOfWork.NotificationRepository.MarkAllAsReadAdminAsync(hotelId);
    }

        public async Task CreateHotelNotificationAsync(int hotelId, NotificationType type, string title, string message, DateTime? eventAtUtc = null,
            string? actorUserId = null, string? recipientUserId = null, int? bookingId = null, int? roomId = null)
        {
            var entity = new Notification
            {
                HotelId = hotelId,
                Type = type,
                Title = title?.Trim() ?? string.Empty,
                Message = message?.Trim() ?? string.Empty,
                EventAtUtc = eventAtUtc,
                ActorUserId = actorUserId,
                RecipientUserId = recipientUserId,
                BookingId = bookingId,
                RoomId = roomId,
                CreatedAtUtc = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.NotificationRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
