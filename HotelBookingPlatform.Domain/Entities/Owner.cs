using System.ComponentModel.DataAnnotations;

namespace HotelBookingPlatform.Domain.Entities
{
    public class Owner
    {
        public int OwnerID { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    }
}