using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBookingPlatform.Domain.Helpers
{
    public static class RoomPricing
    {
        public static decimal GetPrice(RoomType type, BookingDurationType duration)
        {
            // Pricing policy (v1):
            // - 1h/2h => Hours2 bucket
            // - 3h/4h => Hours4 bucket
            // - 5h-8h, Overnight, Stay => Overnight bucket
            // This prevents breaking bookings even if you haven't configured per-hour rates yet.

            var normalized = duration switch
            {
                BookingDurationType.Hours1 or BookingDurationType.Hours2 => BookingDurationType.Hours2,
                BookingDurationType.Hours3 or BookingDurationType.Hours4 => BookingDurationType.Hours4,
                BookingDurationType.Hours5 or BookingDurationType.Hours6 or BookingDurationType.Hours7 or BookingDurationType.Hours8
                    or BookingDurationType.Overnight or BookingDurationType.Stay => BookingDurationType.Overnight,
                _ => throw new ArgumentOutOfRangeException(nameof(duration))
            };

            return type switch
            {
                RoomType.Deluxe => normalized switch
                {
                    BookingDurationType.Hours2 => 2000m,
                    BookingDurationType.Hours4 => 4000m,
                    BookingDurationType.Overnight => 3000m,
                    _ => throw new ArgumentOutOfRangeException(nameof(duration))
                },
                RoomType.Standard => normalized switch
                {
                    BookingDurationType.Hours2 => 1500m,
                    BookingDurationType.Hours4 => 3000m,
                    BookingDurationType.Overnight => 2500m,
                    _ => throw new ArgumentOutOfRangeException(nameof(duration))
                },
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }

}
