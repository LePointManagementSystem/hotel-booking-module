using HotelBookingPlatform.Application.Core.Implementations.BookingManagementService;
using HotelBookingPlatform.Application.Core.Abstracts;
using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IImageService _imageService;
    private readonly IResponseHandler _responseHandler;
    private readonly ILog _logger;
    private readonly IBookingService _bookingService;

    public RoomController(
        IRoomService roomService,
        IImageService imageService,
        IResponseHandler responseHandler,
        ILog logger,
        IBookingService bookingService)
    {
        _roomService = roomService;
        _imageService = imageService;
        _responseHandler = responseHandler;
        _logger = logger;
        _bookingService = bookingService;
    }

    [HttpGet("available")]
    [SwaggerOperation(
        Summary = "Get all available rooms based on date range",
        Description = "Returns rooms where IsAvailable is true and no active booking exists for the specified date range.")]
    public async Task<IActionResult> GetAvailableRoomsByDate(
        [FromQuery] int roomClassId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        // 1. Release expired bookings
        await _bookingService.ReleaseExpiredBookingsAsync();

        // 2. Get available rooms (excluding rooms with active bookings in the date range)
        var rooms = await _roomService.GetAllAvailableRoomsByDateAsync(roomClassId, checkIn, checkOut);

        if (!rooms.Any())
        {
            _logger.Log($"No available rooms found for room class ID {roomClassId} between {checkIn} and {checkOut}.", "warning");
            return _responseHandler.NotFound("No rooms available for the selected period.");
        }

        return _responseHandler.Success(rooms, "Available rooms retrieved successfully.");
    }

    [HttpGet("by-price")]
    [SwaggerOperation(Summary = "Get rooms by price range")]
    [SwaggerResponse(200, "Rooms within the price range retrieved successfully.", typeof(IEnumerable<RoomResponseDto>))]
    [SwaggerResponse(404, "No rooms found within the specified price range.")]
    public async Task<IActionResult> GetRoomsByPriceRange(decimal minPrice, decimal maxPrice)
    {
        var rooms = await _roomService.GetRoomsByPriceRangeAsync(minPrice, maxPrice);

        if (!rooms.Any())
        {
            _logger.Log($"No rooms found within the price range {minPrice} - {maxPrice}.", "warning");
            return _responseHandler.NotFound("No rooms found within the specified price range.");
        }

        return _responseHandler.Success(rooms, "Rooms within the price range retrieved successfully.");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(int id)
    {
        var room = await _roomService.GetRoomAsync(id);
        return _responseHandler.Success(room, "Room retrieved successfully.");
    }

    [HttpPost("{roomId}/upload-image")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Upload an image for a specific room.")]
    public async Task<IActionResult> UploadRoomImage(int roomId, IFormFile file)
    {
        if (file.Length == 0)
        {
            _logger.Log("No file uploaded during room image upload.", "warning");
            return _responseHandler.BadRequest("No file uploaded.");
        }

        var imageType = "Room";
        var uploadResult = await _imageService.UploadImageAsync(file, imageType, roomId);

        return _responseHandler.Success(new
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        }, "Room added successfully to the room class.");
    }

    [HttpDelete("{roomId}/delete-image/{publicId}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete an image from a specific room.")]
    public async Task<IActionResult> DeleteRoomImage(int roomId, string publicId)
    {
        var deletionResult = await _imageService.DeleteImageAsync(publicId);
        return _responseHandler.Success("Image deleted successfully.");
    }

    [HttpGet("{roomId}/images")]
    [SwaggerOperation(Summary = "Retrieve all images associated with a specific room.")]
    public async Task<IActionResult> GetRoomImages(int roomId)
    {
        var allRoomImages = await _imageService.GetImagesByTypeAsync("Room");
        var roomImages = allRoomImages.Where(img => img.EntityId == roomId);

        if (!roomImages.Any())
            return _responseHandler.NotFound("No images found for the specified room.");

        return _responseHandler.Success(roomImages, "Rooms retrieved successfully for the room class.");
    }

    [HttpGet("available-without-bookings")]
    [SwaggerOperation(
        Summary = "Get available rooms with no bookings",
        Description = "Retrieves a list of available rooms that do not have any bookings.")]
    [SwaggerResponse(200, "Available rooms with no bookings retrieved successfully.", typeof(IEnumerable<RoomResponseDto>))]
    [SwaggerResponse(404, "No available rooms found without bookings.")]
    public async Task<IActionResult> GetAvailableRoomsWithNoBookings([FromQuery] int roomClassId)
    {
        var rooms = await _roomService.GetAvailableRoomsWithNoBookingsAsync(roomClassId);

        if (!rooms.Any())
        {
            _logger.Log($"No available rooms found without bookings for room class ID {roomClassId}.", "warning");
            return _responseHandler.NotFound("No available rooms found without bookings.");
        }

        return _responseHandler.Success(rooms, "Available rooms with no bookings retrieved successfully.");
    }
}
