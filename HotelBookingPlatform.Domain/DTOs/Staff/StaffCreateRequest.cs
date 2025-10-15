namespace HotelBookingPlatform.Domain.DTOs.Staff
{
    public class StaffCreateRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int HotelId { get; set; }
        //public string? UserId { get; set; }
        // public bool IsActive { get; set; } = true;
        public bool? IsActive { get; set; }

    }
}