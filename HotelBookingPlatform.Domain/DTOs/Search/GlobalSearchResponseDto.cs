using System;
using System.Collections.Generic;

namespace HotelBookingPlatform.Domain.DTOs.Search
{
    public class GlobalSearchResponseDto
    {
        public List<BookingSearchResultDto> Bookings { get; set; } = new ();

        public List <RoomSearchResultDto> Rooms { get; set; } = new();
        public List<GuestSearchResultDto> Guests { get; set; } = new();

    }

    public class BookingSearchResultDto
    {
        public int BookingId { get; set; } 
        public string confirmationNumber { get; set; } = "";
        public string GuestName { get; set; } = "";
        public string RoomNumbers { get; set; } = "";
        public DateTime checkInDateUtc { get; set; }
        public DateTime checkOutDateUtc { get; set; }
        public string Status { get; set; } = "";
    }

    public class RoomSearchResultDto
    {
        public int RoomId { get; set; }
        public string Number { get; set; } = "";
        public int RoomClassId { get; set; }
        public string RoomClassName { get; set; } = "";
    }

    public class GuestSearchResultDto
    {
        public Guid GuestId { get; set; }
        public string FullName { get; set; } = "";
        public string CIN { get; set; } = "";
        public string Email { get; set; } = "";
    }
}