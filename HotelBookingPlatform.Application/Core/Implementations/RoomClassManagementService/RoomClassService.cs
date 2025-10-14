using HotelBookingPlatform.Application.Core.Abstracts.RoomClassManagementService;
namespace HotelBookingPlatform.Application.Core.Implementations.RoomClassManagementService;
public class RoomClassService : IRoomClassService
{
    private readonly IUnitOfWork<RoomClass> _unitOfWork;
    private readonly IMapper _mapper;

    public RoomClassService(IUnitOfWork<RoomClass> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoomClassResponseDto>> GetAllRoomClassesAsync()
    {
        var roomClasses = await _unitOfWork.RoomClasseRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<RoomClassResponseDto>>(roomClasses);
    }

    public async Task<IEnumerable<RoomClassResponseDto>> GetRoomClassesByHotelId(int hotelId)
    {
        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(hotelId);
        if (hotel is null)
            throw new NotFoundException("Hotel not found.");

        var roomClasses = await _unitOfWork.RoomClasseRepository.GetByHotelIdAsync(hotelId);

        return _mapper.Map<IEnumerable<RoomClassResponseDto>>(roomClasses);
    }
    public async Task<RoomClassResponseDto> CreateRoomClass(RoomClassRequestDto request)
    {
        var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.HotelId);
        if (hotel is null)
            throw new NotFoundException("Hotel not found.");

        var roomClass = _mapper.Map<RoomClass>(request);
        roomClass.HotelId = request.HotelId;

        await _unitOfWork.RoomClasseRepository.CreateAsync(roomClass);

        return _mapper.Map<RoomClassResponseDto>(roomClass);
    }

    public async Task<RoomResponseDto> AddRoomToRoomClassAsync(int roomClassId, RoomCreateRequest request)
    {
        var roomClass = await _unitOfWork.RoomClasseRepository.GetByIdAsync(roomClassId);
        if (roomClass == null)
            throw new NotFoundException("Room class not found.");

        var room = _mapper.Map<Room>(request);
        room.RoomClassID = roomClassId;
        room.CreatedAtUtc = DateTime.UtcNow;
        room.IsAvailable = true;

        await _unitOfWork.RoomRepository.CreateAsync(room);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoomResponseDto>(room);
    }

    public async Task<RoomClassResponseDto> GetRoomClassById(int id)
    {
        var roomClass = await _unitOfWork.RoomClasseRepository.GetByIdAsync(id);
        if (roomClass is null)
            throw new NotFoundException("Room class not found.");

        return _mapper.Map<RoomClassResponseDto>(roomClass);
    }

    public async Task<RoomClassResponseDto> UpdateRoomClass(int id, RoomClassRequestDto request)
    {
        var roomClass = await _unitOfWork.RoomClasseRepository.GetByIdAsync(id);
        if (roomClass is null)
            throw new NotFoundException("Room class not found.");

        _mapper.Map(request, roomClass);
        await _unitOfWork.RoomClasseRepository.UpdateAsync(id,roomClass);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoomClassResponseDto>(roomClass);
    }
}
