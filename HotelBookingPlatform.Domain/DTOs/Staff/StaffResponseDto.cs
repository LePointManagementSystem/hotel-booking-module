namespace HotelBookingPlatform.Domain.DTOs.Staff
{
    public class StaffResponseDto
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int HotelId { get; set; }
        public string? HotelName { get; set; }

        //public string? UserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}