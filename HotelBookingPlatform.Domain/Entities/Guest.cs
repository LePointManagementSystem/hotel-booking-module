using System.ComponentModel.DataAnnotations;

namespace HotelBookingPlatform.Domain.Entities {

    public class Guest {

        public Guid Id {get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^\d{10}$")]
        public string CIN { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public ICollection<Booking> Bookings {get; set; }
    }
}