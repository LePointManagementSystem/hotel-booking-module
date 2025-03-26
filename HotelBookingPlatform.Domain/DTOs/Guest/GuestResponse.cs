namespace HotelBookingPlatform.Domain.DTOs.Guest
{
    public class GuestResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CIN { get; set; }
        public string? Email { get; set; }
    }
}
