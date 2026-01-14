namespace HotelBookingPlatform.API.Profiles;
public class BookingMappingProfile :Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Booking, BookingDto>()
        .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.HotelId))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
        .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
        .ForMember(dest => dest.Numbers, opt => opt.MapFrom(src => src.Rooms.Select(r => r.Number).ToList()))
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
        .ForMember(dest => dest.GuestId, opt => opt.MapFrom(src => src.GuestId))
        .ForMember(dest => dest.GuestFirstName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.FirstName : null))
        .ForMember(dest => dest.GuestLastName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.LastName : null))
        .ForMember(dest => dest.GuestCIN, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.CIN : null))
        .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.CancellationReason))
        .ForMember(dest => dest.CancelledAtUtc, opt => opt.MapFrom(src => src.CancelledAtUtc))
        .ForMember(dest => dest.CancelledByUserId, opt => opt.MapFrom(src => src.CancelledByUserId));


        CreateMap<BookingCreateRequest, Booking>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => BookingStatus.Pending));

 
    }
}
