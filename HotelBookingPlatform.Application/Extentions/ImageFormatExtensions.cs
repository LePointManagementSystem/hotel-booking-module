namespace HotelBookingPlatform.Application.Extentions;

/// <summary>
/// Provides extension methods for converting supported image formats to file extensions.
/// </summary>
public static class ImageFormatExtensions
{
    public static string ToExtension(this SupportedImageFormats format)
    {
        return format switch
        {
            SupportedImageFormats.Jpg => ".jpg",
            SupportedImageFormats.Jpeg => ".jpeg",
            SupportedImageFormats.Png => ".png",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}
