using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
namespace HotelBookingPlatform.Application.Core.Implementations.BookingManagementService;
public class ConfirmationNumberGeneratorService : IConfirmationNumberGeneratorService
{
    public string GenerateConfirmationNumber()
    {
        // 6 caracteres hex(xomme D7A2F0)
        //return Guid.NewGuid().ToString();
        var code = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"BK-{code}";
    }
}
