namespace HotelBookingPlatform.Domain.DTOs.Notification
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int HotelId { get; set; }

        public string Type { get; set; } = string.Empty; // ✅ éviter null

        public string Title { get; set; } = string.Empty; // ✅ éviter null
        public string Message { get; set; } = string.Empty; // ✅ éviter null

        public bool IsRead { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? EventAtUtc { get; set; }
        public DateTime? ReadAtUtc { get; set; }

        public int? BookingId { get; set; }
        public int? RoomId { get; set; }
    }
}
