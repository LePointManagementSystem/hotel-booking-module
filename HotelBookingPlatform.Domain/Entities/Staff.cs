using System.ComponentModel.DataAnnotations;

namespace HotelBookingPlatform.Domain.Entities
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public bool IsActive { get; set; } = true;

        // Facultatif to a user account

        public string? UserId { get; set; }
        public LocalUser? User { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }



    }
}