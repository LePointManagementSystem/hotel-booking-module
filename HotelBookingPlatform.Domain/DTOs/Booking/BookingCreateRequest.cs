namespace HotelBookingPlatform.Domain.DTOs.Booking
{
    public class GuestDto {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string CIN { get; set; }
    }

    public class BookingCreateRequest
    {
        public int HotelId { get; set; }
        public DateTime CheckInDateUtc { get; set; }
        public DateTime CheckOutDateUtc { get; set; }
        public ICollection<int> RoomIds { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public GuestDto Guest { get; set; }
    }
}