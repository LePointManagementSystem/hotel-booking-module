namespace HotelBookingPlatform.Application.Core.Abstracts;

public interface IRoomService
{
    Task<RoomResponseDto> GetRoomAsync(int id);
    Task<RoomResponseDto> AddRoomToRoomClassAsync(int roomClassId, RoomCreateRequest request);

    Task<RoomResponseDto> UpdateRoomAsync(int id, RoomCreateRequest request);
    Task DeleteRoomAsync(int id);
    Task<IEnumerable<RoomResponseDto>> GetAvailableRoomsWithNoBookingsAsync(int roomClassId);
    Task<IEnumerable<RoomResponseDto>> GetRoomsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<RoomResponseDto>> GetAllAvailableRoomsByDateAsync(int roomClassId,DateTime checkIn, DateTime checkOut);
}

