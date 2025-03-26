using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public async Task<ActionResult<IEnumerable<Guest>>> GetGuests()
        {
            return await _dbContext.Guests.ToListAsync();
        }

        // GET: api/Guest/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Guest>> GetGuest(Guid id)
        {
            var guest = await _dbContext.Guests.FindAsync(id);
            if (guest == null)
                return NotFound();

            return guest;
        }

        // POST: api/Guest
        [HttpPost]
        public async Task<ActionResult<Guest>> CreateGuest(Guest guest)
        {
            var existingGuest = await _dbContext.Guests.FirstOrDefaultAsync(g => g.CIN == guest.CIN);
            if (existingGuest != null)
                return Conflict("A guest with this CIN already exists.");

            guest.Id = Guid.NewGuid();
            _dbContext.Guests.Add(guest);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, guest);
        }

        // PUT: api/Guest/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(Guid id, Guest updatedGuest)
        {
            if (id != updatedGuest.Id)
                return BadRequest();

            var guest = await _dbContext.Guests.FindAsync(id);
            if (guest == null)
                return NotFound();

            guest.FirstName = updatedGuest.FirstName;
            guest.LastName = updatedGuest.LastName;
            guest.Email = updatedGuest.Email;
            guest.CIN = updatedGuest.CIN;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Guest/{id}/bookings
        [HttpGet("{id}/bookings")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetGuestBookings(Guid id)
        {
            var guest = await _dbContext.Guests
                .Include(g => g.Bookings)
                .ThenInclude(b => b.Rooms)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guest == null)
                return NotFound();

            return Ok(guest.Bookings);
        }
    }
}
