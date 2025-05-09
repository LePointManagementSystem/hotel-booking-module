using HotelBookingPlatform.Application.Core.Abstracts.IHotelManagementService;
using HotelBookingPlatform.Application.Helpers;
using Org.BouncyCastle.Crypto.Paddings;
namespace HotelBookingPlatform.Application.Core.Implementations.HotelManagementService;
public class CityHotelService : ICityHotelService
{
    private readonly IUnitOfWork<City> _cityUnitOfWork;
    private readonly IUnitOfWork<Hotel> _hotelUnitOfWork;
    private readonly IUnitOfWork<Owner> _ownerUnitOfWork;
    private readonly IMapper _mapper;
    private readonly ILog _logger;
    private readonly EntityValidator<City> _cityValidator;
    private readonly EntityValidator<Hotel> _hotelValidator;
    private readonly EntityValidator<Owner> _ownerValidator;

    public CityHotelService(
        IUnitOfWork<City> cityUnitOfWork,
        IUnitOfWork<Hotel> hotelUnitOfWork,
        IUnitOfWork<Owner> ownerUnitOfWork,
        IMapper mapper,
        ILog logger)
    {
        _cityUnitOfWork = cityUnitOfWork ?? throw new ArgumentNullException(nameof(cityUnitOfWork));
        _hotelUnitOfWork = hotelUnitOfWork ?? throw new ArgumentNullException(nameof(hotelUnitOfWork));
        _ownerUnitOfWork = ownerUnitOfWork ?? throw new ArgumentNullException(nameof(ownerUnitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cityValidator = new EntityValidator<City>(_cityUnitOfWork.CityRepository);
        _hotelValidator = new EntityValidator<Hotel>(_hotelUnitOfWork.HotelRepository);
        _ownerValidator = new EntityValidator<Owner>(_ownerUnitOfWork.OwnerRepository);
    }

    public async Task AddHotelToCityAsync(int cityId, HotelCreateRequest hotelRequest)
    {
        try {

        var city = await _cityValidator.ValidateExistenceAsync(cityId);
        if (city is null)
        {
            _logger.Log($"City with ID {cityId} not found.", "error");
            throw new KeyNotFoundException($"City with ID {cityId} not found.");
        }

        var owner = await _ownerValidator.ValidateExistenceAsync(hotelRequest.OwnerID);
        if (owner is null) {

            _logger.Log($"Owner with ID {hotelRequest.OwnerID} not found.", "error");
            throw new KeyNotFoundException($"Owner with ID {hotelRequest.OwnerID} not found.");
        }

        var hotel = _mapper.Map<Hotel>(hotelRequest);
        hotel.CityID = cityId;
        hotel.OwnerID = hotelRequest.OwnerID;

        await _hotelUnitOfWork.HotelRepository.CreateAsync(hotel);

        if(city.Hotels == null)
            city.Hotels = new List<Hotel>();

        city.Hotels.Add(hotel);
        await _hotelUnitOfWork.SaveChangesAsync();
        await _cityUnitOfWork.SaveChangesAsync();

        _logger.Log($"Added hotel to city with ID {cityId}. Hotel Name: {hotel.Name}.", "info");

        }
        catch (Exception ex) {
            _logger.Log($"Error adding hotel: {ex.Message}", "error");
        }
    }

    public async Task DeleteHotelFromCityAsync(int cityId, int hotelId)
    {
        var city = await _cityValidator.ValidateExistenceAsync(cityId);
        var hotel = await _hotelValidator.ValidateExistenceAsync(hotelId);

        if (hotel.CityID != cityId)
            throw new KeyNotFoundException($"Hotel with ID {hotelId} does not belong to city with ID {cityId}.");

        await _hotelUnitOfWork.HotelRepository.DeleteAsync(hotelId);
        city.Hotels.Remove(hotel);

        await _cityUnitOfWork.SaveChangesAsync();
        _logger.Log($"Removed hotel with ID {hotelId} from city with ID {cityId}.", "info");
    }

    public async Task<IEnumerable<HotelResponseDto>> GetHotelsForCityAsync(int cityId)
    {
        var hotels = await _hotelUnitOfWork.HotelRepository.GetHotelsForCityAsync(cityId);

        if (hotels is null || !hotels.Any())
            throw new InvalidOperationException($"No hotels found for city with ID {cityId}.");

        _logger.Log($"Retrieved {hotels.Count()} hotels for city with ID {cityId}.", "info");
        return _mapper.Map<IEnumerable<HotelResponseDto>>(hotels);
    }
}

