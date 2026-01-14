namespace HotelBookingPlatform.Domain.DTOs.Reports;

public sealed class CancellationReasonCountDto
{
    public string Reason { get; set; } = string.Empty;
    public int Count { get; set; }
}
