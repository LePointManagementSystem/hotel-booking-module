using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBookingPlatform.Domain.Enums
{
    public enum BookingDurationType
    {
        Hours2 = 0,
        Hours4 = 1,
        Overnight = 2,

        // Additional hourly durations
        Hours1 =3,
        Hours3 = 4,
        Hours5 = 5,
        Hours6 = 6,
        Hours7 = 7,
        Hours8 = 8,

        // Multi-day Stay(24h+). Provides checkout date in frontend
        Stay = 9
    }
}
