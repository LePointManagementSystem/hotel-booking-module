namespace HotelBookingPlatform.API.Profiles;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomResponseDto>()
            .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomID)) // selon ton entity
            .ForMember(dest => dest.RoomClassId, opt => opt.MapFrom(src => src.RoomClassID))
            .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.RoomClass.HotelId))
            .ForMember(dest => dest.RoomClassName, opt => opt.MapFrom(src => src.RoomClass.Name));
    }
}
