namespace HotelBookingPlatform.Domain.Enums
{
    public enum NotificationType
    {
        // NOTE: keep numeric values stable for DB storage
        BookingConfirmed = 1,
        BookingCompleted = 2,
        BookingCancelled = 3,
        System = 99
    }
}