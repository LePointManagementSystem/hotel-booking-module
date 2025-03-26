using HotelBookingPlatform.Application.Core.Abstracts;
using HotelBookingPlatform.Domain.DTOs.Guest;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBookingPlatform.Application.Services
{
    public class GuestService : IGuestService
    {
        private readonly AppDbContext _context;

        public GuestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GuestResponse> CreateGuestAsync(GuestRequest request)
        {
            var existingGuest = await _context.Guests.FirstOrDefaultAsync(g => g.CIN == request.CIN);
            if (existingGuest != null)
            {
                return new GuestResponse
                {
                    Id = existingGuest.Id,
                    FirstName = existingGuest.FirstName,
                    LastName = existingGuest.LastName,
                    CIN = existingGuest.CIN,
                    Email = existingGuest.Email
                };
            }

            var guest = new Guest
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                CIN = request.CIN,
                Email = request.Email
            };

            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();

            return new GuestResponse
            {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                CIN = guest.CIN,
                Email = guest.Email
            };
        }

        public async Task<IEnumerable<GuestResponse>> GetAllGuestsAsync()
        {
            return await _context.Guests
                .Select(g => new GuestResponse
                {
                    Id = g.Id,
                    FirstName = g.FirstName,
                    LastName = g.LastName,
                    CIN = g.CIN,
                    Email = g.Email
                })
                .ToListAsync();
        }

        public async Task<GuestResponse> GetGuestByIdAsync(Guid id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null) return null!;

            return new GuestResponse
            {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                CIN = guest.CIN,
                Email = guest.Email
            };
        }

        public async Task<bool> DeleteGuestAsync(Guid id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null) return false;

            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
