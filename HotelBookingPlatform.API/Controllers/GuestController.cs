using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotelBookingPlatform.Domain.DTOs.Guest;

namespace HotelBookingPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class GuestController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public GuestController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/Guest
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuestResponse>>> GetGuests()
        {
            //return await _dbContext.Guests.ToListAsync();
            var guests = await _dbContext.Guests
                .Select(g => new GuestResponse {
                    Id = g.Id,
                    FirstName = g.FirstName,
                    LastName = g.LastName,
                    CIN = g.CIN,
                    Email = g.Email
                })
                .ToListAsync();

                return Ok(guests);
        }

        // GET: api/Guest/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GuestResponse>> GetGuest(Guid id)
        {
            var guest = await _dbContext.Guests.FindAsync(id);
            if (guest == null)
                return NotFound();

            var response = new GuestResponse {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                CIN = guest.CIN,
                Email = guest.Email
            };

            return Ok(response);
        }

        // POST: api/Guest
        [HttpPost]
        public async Task<ActionResult<GuestResponse>> CreateGuest([FromBody] GuestRequest request)
        {
            var existingGuest = await _dbContext.Guests.FirstOrDefaultAsync(g => g.CIN == request.CIN);
            if (existingGuest != null)
                return Conflict("A guest with this CIN already exists.");

            var guest = new Guest {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                CIN = request.CIN,
                Email = request.Email
            };

            _dbContext.Guests.Add(guest);
            await _dbContext.SaveChangesAsync();

            var response = new GuestResponse {

                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                CIN = guest.CIN,
                Email = guest.Email
            };
            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id}, response);
        }

        // PUT: api/Guest/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(Guid id, [FromBody] GuestRequest request)
        {
            var guest = await _dbContext.Guests.FindAsync(id);
            if (guest == null)
                return NotFound();

                guest.FirstName = request.FirstName;
                guest.LastName = request.LastName;
                guest.CIN = request.CIN;
                guest.Email = request.Email;
                
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Guest/{id}/bookings
        [HttpGet("{id}/bookings")]
        public async Task<ActionResult<IEnumerable<object>>> GetGuestBookings(Guid id) {

            var guest = await _dbContext.Guests
                .Include(g => g.Bookings)
                .ThenInclude(b => b.Rooms)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guest == null)
                return NotFound();

            var bookings = guest.Bookings.Select(b => new {
                b.BookingID,
                b.Status,
                b.ConfirmationNumber,
                b.TotalPrice,
                b.BookingDateUtc,
                b.CheckInDateUtc,
                b.CheckOutDateUtc

            });
            
            return Ok(bookings);
        }
    }
}
