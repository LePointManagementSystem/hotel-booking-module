using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Implementation
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Notification>> GetForHotelAsync(
            int hotelId, string? userId, bool includeRead, int skip, int take)
        {
            IQueryable<Notification> query = _context.Set<Notification>()
                .AsNoTracking()
                .Where(n => n.HotelId == hotelId)
                .Where(n => n.RecipientUserId == null || n.RecipientUserId == userId);

            if (!includeRead)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAtUtc)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountUnreadAsync(int hotelId, string? userId)
        {
            return await _context.Set<Notification>()
                .AsNoTracking()
                .Where(n => n.HotelId == hotelId)
                .Where(n => (n.RecipientUserId == null || n.RecipientUserId == userId) && !n.IsRead)
                .CountAsync();
        }

        public async Task<Notification?> GetScopedByIdAsync(int notificationId, int hotelId, string? userId)
        {
            return await _context.Set<Notification>()
                .AsTracking()
                .Where(n => n.NotificationId == notificationId && n.HotelId == hotelId)
                .Where(n => n.RecipientUserId == null || n.RecipientUserId == userId)
                .FirstOrDefaultAsync();
        }

        // ✅ Admin/Manager: lire toutes les notifications (option filtre hotelId)
        public async Task<IReadOnlyList<Notification>> GetAllAsync(int? hotelId, bool includeRead, int skip, int take)
        {
            IQueryable<Notification> query = _context.Set<Notification>()
                .AsNoTracking();

            if (hotelId.HasValue && hotelId.Value > 0)
                query = query.Where(n => n.HotelId == hotelId.Value);

            if (!includeRead)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAtUtc)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountUnreadAllAsync(int? hotelId)
        {
            IQueryable<Notification> query = _context.Set<Notification>()
                .AsNoTracking()
                .Where(n => !n.IsRead);

            if (hotelId.HasValue && hotelId.Value > 0)
                query = query.Where(n => n.HotelId == hotelId.Value);

            return await query.CountAsync();
        }

        // ✅ Important : on implémente explicitement pour éviter le warning de nullability
        async Task<Notification?> INotificationRepository.GetByIdAsync(int notificationId)
        {
            return await _context.Set<Notification>()
                .AsTracking()
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }

        public async Task MarkAsReadAsync(Notification notification)
        {
            if (notification.IsRead) return;

            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int hotelId, string? userId)
        {
            var items = await _context.Set<Notification>()
                .AsTracking()
                .Where(n => n.HotelId == hotelId)
                .Where(n => n.RecipientUserId == null || n.RecipientUserId == userId)
                .Where(n => !n.IsRead)
                .ToListAsync();

            if (items.Count == 0) return;

            var now = DateTime.UtcNow;
            foreach (var n in items)
            {
                n.IsRead = true;
                n.ReadAtUtc = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAdminAsync(int? hotelId)
        {
            IQueryable<Notification> query = _context.Set<Notification>()
                .AsTracking()
                .Where(n => !n.IsRead);

            if (hotelId.HasValue && hotelId.Value > 0)
                query = query.Where(n => n.HotelId == hotelId.Value);

            var items = await query.ToListAsync();
            if (items.Count == 0) return;

            var now = DateTime.UtcNow;
            foreach (var n in items)
            {
                n.IsRead = true;
                n.ReadAtUtc = now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
