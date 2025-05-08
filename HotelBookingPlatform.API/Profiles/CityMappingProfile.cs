namespace HotelBookingPlatform.API.Profiles;

public class CityMappingProfile : Profile
{
    public CityMappingProfile()
    {
        CreateMap<CityCreateRequest, City>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.PostOffice, opt => opt.MapFrom(src => src.PostOffice))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<City, CityResponseDto>();
        CreateMap<City, CityWithHotelsResponseDto>()
            .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels));
        CreateMap<Hotel, HotelResponseDto>();
    }
}
