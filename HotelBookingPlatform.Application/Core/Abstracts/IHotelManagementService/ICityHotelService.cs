namespace HotelBookingPlatform.Application.Core.Abstracts.IHotelManagementService;
public interface ICityHotelService
{
    Task<IEnumerable<HotelResponseDto>> GetHotelsForCityAsync(int cityId);
    Task AddHotelToCityAsync(int cityId, HotelCreateRequest hotelRequest);
    Task DeleteHotelFromCityAsync(int cityId, int hotelId);
}
