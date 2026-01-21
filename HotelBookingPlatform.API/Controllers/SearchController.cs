using HotelBookingPlatform.Domain.DTOs.Search;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController: ControllerBase
    {
        private readonly AppDbContext _db;

        public SearchController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/Search/global?q=Larsson&hotelId=1&limit=6
        [HttpGet("global")]
        public async Task<ActionResult<GlobalSearchResponseDto>> Global(
            [FromQuery] string q,
            [FromQuery] int? hotelId = null,
            [FromQuery] int limit = 6
        )

        {
            q = (q ?? "").Trim();
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new GlobalSearchResponseDto());
            
            limit = Math.Clamp(limit, 3, 20);
            var qLower = q.ToLower();

            // optional: if q looks like BK-XXXXXX, remove BK-
            var qNoPrefix = qLower.StartsWith("bk-") ? qLower.Substring(3) : qLower;

            //Try parse bookingId
            var bookingIdParsed = int.TryParse(q, out var bookingId) ? bookingId : (int?)null;

            // --------- Bookings ----------------
            var bookingsQuery = _db.Bookings
                .AsNoTracking()
                .Include(b => b.Guest)
                .Include(b => b.Rooms)
                .AsQueryable();

            if (hotelId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.HotelId == hotelId.Value);

            bookingsQuery = bookingsQuery.Where(b => 
            (bookingIdParsed.HasValue && b.BookingID == bookingIdParsed.Value)
            || (!string.IsNullOrEmpty(b.ConfirmationNumber) && b.ConfirmationNumber.ToLower().Contains(qLower))
            || (!string.IsNullOrEmpty(b.ConfirmationNumber) && b.ConfirmationNumber.ToLower().Contains(qNoPrefix))
            || (b.Guest != null && 
            (
                (b.Guest.FirstName != null && b.Guest.FirstName.ToLower().Contains(qLower))
                || (b.Guest.LastName != null && b.Guest.LastName.ToLower().Contains(qLower))
                || ((b.Guest.FirstName + " " + b.Guest.LastName).ToLower().Contains(qLower))
                || (b.Guest.CIN != null && b.Guest.CIN.ToLower().Contains(qLower))
                || (b.Guest.Email != null && b.Guest.Email.ToLower().Contains(qLower))

            )
            
            )
            || (b.Rooms != null && b.Rooms.Any(r =>
                r.Number != null && r.Number.ToLower().Contains(qLower)
                ))
            
            );

            var bookings = await bookingsQuery
                .OrderByDescending(b => b.BookingDateUtc)
                .Take(limit)
                .Select(b => new BookingSearchResultDto
                {
                    BookingId = b.BookingID,
                    confirmationNumber = b.ConfirmationNumber ?? "",
                    GuestName = b.Guest != null ? (b.Guest.FirstName + " " + b.Guest.LastName) : "",
                    RoomNumbers = b.Rooms != null ? string.Join(", ", b.Rooms.Select(r => r.Number)) : "",
                    checkInDateUtc = b.CheckInDateUtc,
                    checkOutDateUtc = b.CheckOutDateUtc,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

                // --------- Rooms ------------
                var roomsQuery = _db.Rooms
                    .AsNoTracking()
                    .Include(r => r.RoomClass)
                    .AsQueryable();

                if (hotelId.HasValue)
            {
                // Important: adapt if Room has HotelId in your model
                // If Room doesn't have HotelId, remove this filter.
                // roomsQuery = roomsQuery.Where(r => r.HotelId == hotelId.Value);

            }

            var rooms = await roomsQuery
                .Where(r => 
                r.Number != null && r.Number.ToLower().Contains(qLower)
                )
                .OrderBy(r => r.Number)
                .Take(limit)
                .Select(r => new RoomSearchResultDto
                {
                    RoomId = r.RoomID,
                    Number = r.Number ?? "",
                    RoomClassId = r.RoomClassID,
                    RoomClassName = r.RoomClass != null ? r.RoomClass.Name : ""

                })
                .ToListAsync();

                // ---------- Guests -----------
                var guestsQuery = _db.Guests.AsNoTracking().AsQueryable();
                
                var guests = await guestsQuery
                    .Where(g => 
                    (g.FirstName != null && g.FirstName.ToLower().Contains(qLower))
                    || (g.LastName != null && g.LastName.ToLower().Contains(qLower))
                    || ((g.FirstName + " " + g.LastName).ToLower().Contains(qLower))
                    || (g.CIN != null && g.CIN.ToLower().Contains(qLower))
                    || (g.Email != null && g.Email.ToLower().Contains(qLower))
                    )
                    .OrderBy(g => g.LastName)
                    .Take(limit)
                    .Select(g => new GuestSearchResultDto
                    {
                        GuestId = g.Id,
                        FullName = (g.FirstName ?? "") + " " + (g.LastName ?? ""),
                        CIN = g.CIN ?? "",
                        Email = g.Email ?? ""
                    })
                    .ToListAsync();

                return Ok(new GlobalSearchResponseDto
                {
                    Bookings = bookings,
                    Rooms = rooms,
                    Guests = guests
                });
        }
    }
}
