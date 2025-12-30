namespace HotelBookingPlatform.Domain.DTOs.Booking
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int HotelId { get; set;}
        public string UserName { get; set; }
        public string ConfirmationNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime BookingDateUtc { get; set; }
        public string PaymentMethod { get; set; }
        public decimal? AfterDiscountedPrice { get; set; }
        public string HotelName { get; set; }
        public DateTime CheckInDateUtc { get; set; }
        public DateTime CheckOutDateUtc { get; set; }
        public string Status { get; set; }
        public List<string> Numbers { get; set; }
        public string DurationType { get; set; }

        // Guest info (for confirmation screen / reception)
        public Guid? GuestId { get; set; }
        public string? GuestFirstName { get; set; }
        public string? GuestLastName { get; set; }
        public string? GuestCIN { get; set; }
    }
}