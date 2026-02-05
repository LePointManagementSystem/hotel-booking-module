using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Manager,Staff")]
public class HotelController : ControllerBase
{
    private readonly IHotelManagementService _hotelManagementService;
    private readonly IHotelSearchService _hotelSearchService;
    private readonly IHotelAmenityService _hotelAmenityService;
    private readonly IHotelReviewService _hotelReviewService;
    private readonly IImageService _imageService;
    private readonly IResponseHandler _responseHandler;
    private readonly IHotelRoomService _hotelRoomService;
    private readonly ILog _logger;
    private readonly IRoomClassService _roomClassService;

    public HotelController(
        IHotelManagementService hotelManagementService,
        IHotelSearchService hotelSearchService,
        IHotelAmenityService hotelAmenityService,
        IHotelReviewService hotelReviewService,
        IImageService imageService,
        IResponseHandler responseHandler,
        ILog logger,
        IHotelRoomService hotelRoomService,
        IRoomClassService roomClassService)
    {
        _hotelManagementService = hotelManagementService ?? throw new ArgumentNullException(nameof(hotelManagementService));
        _hotelSearchService = hotelSearchService ?? throw new ArgumentNullException(nameof(hotelSearchService));
        _hotelAmenityService = hotelAmenityService ?? throw new ArgumentNullException(nameof(hotelAmenityService));
        _hotelReviewService = hotelReviewService ?? throw new ArgumentNullException(nameof(hotelReviewService));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _responseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hotelRoomService = hotelRoomService ?? throw new ArgumentNullException(nameof(hotelRoomService));
        _roomClassService = roomClassService ?? throw new ArgumentNullException(nameof(roomClassService));
    }

    private int? GetScopedHotelId()
    {
        var v = User.FindFirst("hotelId")?.Value;
        return int.TryParse(v, out var id) ? id : null;
    }

    private IActionResult? ForbidIfStaffOtherHotel(int hotelId)
    {
        if (!User.IsInRole("Staff")) return null;
        var scoped = GetScopedHotelId();
        if (!scoped.HasValue) return Forbid();
        if (scoped.Value != hotelId) return Forbid("You cannot access another hotel.");
        return null;
    }

    // =========================
    // READ
    // =========================

    [HttpGet("{id}")]
    [Authorize(Roles="Admin,Manager,Staff")]
    [ResponseCache(CacheProfileName = "DefaultCache")]
    [SwaggerOperation(Summary = "Get a hotel by ID", Description = "Retrieves the details of a specific hotel by its ID.")]
    public async Task<IActionResult> GetHotel(int id)
    {
        var forbid = ForbidIfStaffOtherHotel(id);
        if (forbid != null) return forbid;

        var hotel = await _hotelManagementService.GetHotel(id);
        return _responseHandler.Success(hotel, "Hotel retrieved successfully.");
    }

    // (Optionnel public) => mets [AllowAnonymous] si tu veux que le site public puisse chercher les hôtels
    [HttpGet("search")]
    [Authorize(Roles="Admin,Manager")]

    [ResponseCache(CacheProfileName = "DefaultCache")]
    [SwaggerOperation(Summary = "Search for hotels", Description = "Searches for hotels based on name and description with pagination.")]
    public async Task<IActionResult> SearchHotel(
        [FromQuery] string? name = "",
        [FromQuery] string? desc = "",
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1)
    {
        // Si Staff : tu peux forcer une recherche uniquement sur SON hôtel (selon ton besoin)
        if (User.IsInRole("Staff"))
        {
            var hid = GetScopedHotelId();
            if (!hid.HasValue) return Forbid();

            // Si tu as une méthode GetHotel(id), tu peux juste renvoyer son hôtel
            var hotel = await _hotelManagementService.GetHotel(hid.Value);
            return _responseHandler.Success(new[] { hotel });
        }

        var hotels = await _hotelSearchService.GetHotels(name ?? "", desc ?? "", pageSize, pageNumber);
        return _responseHandler.Success(hotels);
    }

    [HttpGet("{hotelId}/rooms")]
    [SwaggerOperation(Summary = "Get all rooms associated with a specific hotel.")]
    public async Task<IActionResult> GetRoomsByHotelIdAsync(int hotelId)
    {
        var forbid = ForbidIfStaffOtherHotel(hotelId);
        if (forbid != null) return forbid;

        _logger.Log($"Fetching rooms for hotel with ID {hotelId}", "info");
        var rooms = await _hotelRoomService.GetRoomsByHotelIdAsync(hotelId);
        return _responseHandler.Success(rooms, "Rooms retrieved successfully.");
    }

    [HttpGet("{hotelId}/roomclasses")]
    [SwaggerOperation(Summary = "Retrieve all RoomClasses for a specific hotel.")]
    public async Task<IActionResult> GetRoomClassesByHotelId(int hotelId)
    {
        var forbid = ForbidIfStaffOtherHotel(hotelId);
        if (forbid != null) return forbid;

        var roomClassesDto = await _roomClassService.GetRoomClassesByHotelId(hotelId);
        return _responseHandler.Success(roomClassesDto, "Room classes retrieved successfully.");
    }

    [HttpGet("{hotelId}/amenities")]
    [ResponseCache(CacheProfileName = "DefaultCache")]
    [SwaggerOperation(Summary = "Get all amenities associated with a specific hotel.")]
    public async Task<IActionResult> GetAmenitiesByHotelId(int hotelId)
    {
        var forbid = ForbidIfStaffOtherHotel(hotelId);
        if (forbid != null) return forbid;

        _logger.Log($"Fetching amenities for hotel with ID {hotelId}", "info");
        var amenities = await _hotelAmenityService.GetAmenitiesByHotelIdAsync(hotelId);
        return _responseHandler.Success(amenities, "Amenities retrieved successfully.");
    }

    [HttpGet("{hotelId}/reviews")]
    [ResponseCache(CacheProfileName = "DefaultCache")]
    [SwaggerOperation(Summary = "Get all reviews for a specific hotel.")]
    public async Task<IActionResult> GetHotelReviews(int hotelId)
    {
        var forbid = ForbidIfStaffOtherHotel(hotelId);
        if (forbid != null) return forbid;

        var comments = await _hotelReviewService.GetHotelCommentsAsync(hotelId);
        return _responseHandler.Success(comments, "Reviews retrieved successfully.");
    }

    [HttpGet("{id}/rating")]
    [ResponseCache(CacheProfileName = "DefaultCache")]
    [SwaggerOperation(Summary = "Get the review rating of a specific hotel.")]
    public async Task<IActionResult> GetHotelReviewRating(int id)
    {
        var forbid = ForbidIfStaffOtherHotel(id);
        if (forbid != null) return forbid;

        var ratingDto = await _hotelReviewService.GetHotelReviewRatingAsync(id);
        return _responseHandler.Success(ratingDto, "Retrieved successfully.");
    }

    [HttpGet("{hotelId}/images")]
    [SwaggerOperation(Summary = "Retrieve all images associated with a specific hotel.")]
    public async Task<IActionResult> GetImagesForHotel(int hotelId)
    {
        var forbid = ForbidIfStaffOtherHotel(hotelId);
        if (forbid != null) return forbid;

        var hotelImages = await _imageService.GetImagesByTypeAsync("Hotels");
        var images = hotelImages.Where(img => img.EntityId == hotelId).ToList();

        if (!images.Any())
            return _responseHandler.NotFound("No images found for the specified hotel.");

        return _responseHandler.Success(new
        {
            Images = images,
            Message = "Images retrieved successfully for the hotel."
        });
    }

    // =========================
    // WRITE (Admin only)
    // =========================

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelResponseDto request)
    {
        var updatedHotel = await _hotelManagementService.UpdateHotelAsync(id, request);
        return _responseHandler.Success(updatedHotel, "Updated successfully.");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        await _hotelManagementService.DeleteHotel(id);
        return _responseHandler.Success("Hotel deleted successfully.");
    }

    [HttpPost("{hotelId}/amenities")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddAmenityToHotel(int hotelId, [FromBody] AmenityCreateRequest request)
    {
        var amenityDto = await _hotelAmenityService.AddAmenityToHotelAsync(hotelId, request);
        return _responseHandler.Created(amenityDto, "Amenity added successfully.");
    }

    [HttpDelete("{hotelId}/amenities/{amenityId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAmenityFromHotel(int hotelId, int amenityId)
    {
        await _hotelAmenityService.DeleteAmenityFromHotelAsync(hotelId, amenityId);
        return _responseHandler.Success("Amenity deleted successfully.");
    }

    [HttpPost("{hotelId}/upload-image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadHotelImage(int hotelId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return _responseHandler.BadRequest("No file uploaded.");

        var uploadResult = await _imageService.UploadImageAsync(file, "Hotels", hotelId);

        return _responseHandler.Success(new { Url = uploadResult.SecureUrl.ToString() },
            "Image uploaded successfully for the hotel.");
    }

    [HttpDelete("{hotelId}/delete-image/{publicId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHotelImage(int hotelId, string publicId)
    {
        await _imageService.DeleteImageAsync(publicId);
        return _responseHandler.Success("Image deleted successfully.");
    }

    [HttpPost("{hotelId}/roomclasses")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRoomClassToHotel(int hotelId, [FromBody] RoomClassRequestDto request)
    {
        // IMPORTANT: éviter qu’on envoie un HotelId différent dans le body
        request.HotelId = hotelId;

        var roomClassDto = await _roomClassService.CreateRoomClass(request);
        return _responseHandler.Created(roomClassDto, "RoomClass added successfully.");
    }
}
