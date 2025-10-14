namespace HotelBookingPlatform.Application.Core.Abstracts.RoomClassManagementService;

public interface IRoomClassService
{
    Task<RoomResponseDto> AddRoomToRoomClassAsync(int roomClassId, RoomCreateRequest request);
    Task<RoomClassResponseDto> CreateRoomClass(RoomClassRequestDto request);
    Task<RoomClassResponseDto> GetRoomClassById(int id);
    Task<RoomClassResponseDto> UpdateRoomClass(int id, RoomClassRequestDto request);
    Task<IEnumerable<RoomClassResponseDto>> GetRoomClassesByHotelId(int hotelId);
    Task<IEnumerable<RoomClassResponseDto>> GetAllRoomClassesAsync();
}
