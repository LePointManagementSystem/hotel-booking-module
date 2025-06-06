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
            return type switch
            {
                RoomType.Deluxe => duration switch
                {
                    BookingDurationType.Hours2 => 2000m,
                    BookingDurationType.Hours4 => 4000m,
                    BookingDurationType.Overnight => 3000m,
                    _ => throw new ArgumentOutOfRangeException(nameof(duration))
                },
                RoomType.Standard => duration switch
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
