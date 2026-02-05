using System.ComponentModel.DataAnnotations;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        // destinataire (si null => notification "globale" pour l’hôtel)
        public string? RecipientUserId { get; set; }
        public LocalUser? RecipientUser { get; set; }

        // utilisateur qui a déclenché l'action (optionnel)
        public string? ActorUserId { get; set; }
        public LocalUser? ActorUser { get; set; }

        public NotificationType Type { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? EventAtUtc { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAtUtc { get; set; }

        public int? BookingId { get; set; }
        public int? RoomId { get; set; }
    }
}
