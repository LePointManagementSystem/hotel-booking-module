using HotelBookingPlatform.Application.Core.Abstracts.RoomClassManagementService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Manager,Staff")]
public class RoomClassController : ControllerBase
{
    private readonly ILogger<RoomClassController> _logger;
    private readonly IRoomClassService _roomClassService;
    private readonly IRoomManagementService _roomManagementService;
    private readonly IAmenityManagementService _amenityManagementService;
    private readonly IImageService _imageService;
    private readonly IResponseHandler _responseHandler;

    public RoomClassController(
        IRoomClassService roomClassService,
        IRoomManagementService roomManagementService,
        IAmenityManagementService amenityManagementService,
        IImageService imageService,
        IResponseHandler responseHandler,
        ILogger<RoomClassController> logger)
    {
        _roomClassService = roomClassService;
        _roomManagementService = roomManagementService;
        _amenityManagementService = amenityManagementService;
        _imageService = imageService;
        _responseHandler = responseHandler;
        _logger = logger;
    }

    private int? GetScopedHotelId()
    {
        var v = User.FindFirst("hotelId")?.Value;
        return int.TryParse(v, out var id) ? id : null;
    }

    private async Task<IActionResult?> EnsureStaffOwnsRoomClass(int roomClassId)
    {
        if (!User.IsInRole("Staff")) return null;

        var scopedHotelId = GetScopedHotelId();
        if (!scopedHotelId.HasValue) return Forbid();

        var roomClass = await _roomClassService.GetRoomClassById(roomClassId);

        // RoomClassResponseDto contient HotelId ✅
        if (roomClass.HotelId != scopedHotelId.Value)
            return Forbid("You cannot access another hotel.");

        return null;
    }

    // =========================
    // WRITE (Admin only)
    // =========================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRoomClass([FromBody] RoomClassRequestDto request)
    {
        if (!ModelState.IsValid)
            return _responseHandler.BadRequest("Invalid request data.");

        var createdRoomClass = await _roomClassService.CreateRoomClass(request);
        return _responseHandler.Created(createdRoomClass, "Room class created successfully.");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRoomClass(int id, [FromBody] RoomClassRequestDto request)
    {
        var updatedRoomClass = await _roomClassService.UpdateRoomClass(id, request);
        return _responseHandler.Success(updatedRoomClass, "Room class updated successfully.");
    }

    [HttpPost("{roomClassId}/rooms")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRoomToRoomClass(int roomClassId, [FromBody] RoomCreateRequest request)
    {
        var roomDto = await _roomManagementService.AddRoomToRoomClassAsync(roomClassId, request);
        return _responseHandler.Created(roomDto, "Room added successfully.");
    }

    [HttpDelete("{roomClassId}/rooms/{roomId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoomFromRoomClass(int roomClassId, int roomId)
    {
        await _roomManagementService.DeleteRoomFromRoomClassAsync(roomClassId, roomId);
        return _responseHandler.NoContent("Room deleted successfully.");
    }

    [HttpPost("{roomClassId}/addamenity")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddAmenityToRoomClass(int roomClassId, [FromBody] AmenityCreateDto request)
    {
        var addedAmenity = await _amenityManagementService.AddAmenityToRoomClassAsync(roomClassId, request);
        return _responseHandler.Created(addedAmenity, "Amenity added successfully to the room class.");
    }

    [HttpDelete("{roomClassId}/amenities/{amenityId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAmenityFromRoomClass(int roomClassId, int amenityId)
    {
        await _amenityManagementService.DeleteAmenityFromRoomClassAsync(roomClassId, amenityId);
        return _responseHandler.NoContent("Amenity deleted successfully.");
    }

    [HttpPost("{roomClassId}/upload-image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadRoomClassImage(int roomClassId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return _responseHandler.BadRequest("No file uploaded.");

        var uploadResult = await _imageService.UploadImageAsync(file, "roomClass", roomClassId);

        return _responseHandler.Success(
            new { Url = uploadResult.SecureUrl.ToString(), PublicId = uploadResult.PublicId },
            "Image uploaded successfully for the room class.");
    }

    [HttpDelete("{roomClassId}/delete-image/{publicId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoomClassImage(int roomClassId, string publicId)
    {
        await _imageService.DeleteImageAsync(publicId);
        return _responseHandler.Success("Image deleted successfully.");
    }

    // =========================
    // READ (Admin/Manager full, Staff scoped)
    // =========================

    [HttpGet]
    [SwaggerOperation(Summary = "Get all room classes", Description = "Retrieve all room class types")]
    public async Task<IActionResult> GetAllRoomClasses()
    {
        try
        {
            if (User.IsInRole("Staff"))
            {
                var hid = GetScopedHotelId();
                if (!hid.HasValue) return Forbid();

                var scoped = await _roomClassService.GetRoomClassesByHotelId(hid.Value);
                return _responseHandler.Success(scoped);
            }

            var roomClasses = await _roomClassService.GetAllRoomClassesAsync();
            return _responseHandler.Success(roomClasses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving room classes");
            return _responseHandler.BadRequest("An error occurred while retrieving room classes.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomClass(int id)
    {
        // Staff scope check
        var forbid = await EnsureStaffOwnsRoomClass(id);
        if (forbid != null) return forbid;

        var roomClass = await _roomClassService.GetRoomClassById(id);
        return _responseHandler.Success(roomClass, "Room class retrieved successfully.");
    }

    [HttpGet("{roomClassId}/rooms")]
    [SwaggerOperation(Summary = "Get all rooms for a specific room class")]
    

    public async Task<IActionResult> GetRoomsByRoomClassId(int roomClassId)
    {
        if (User.IsInRole("Staff"))
        {
            var scoped = User.FindFirst("hotelId")?.Value;
            if (!int.TryParse(scoped, out var hid)) return Forbid();

            var rc = await _roomClassService.GetRoomClassById(roomClassId);
            if (rc.HotelId != hid) return Forbid("You cannot access another hotel.");
        }

        var rooms = await _roomManagementService.GetRoomsByRoomClassIdAsync(roomClassId);
        return _responseHandler.Success(rooms, "Rooms retrieved successfully for the room class.");
    }


    [HttpGet("{roomClassId}/amenities")]
    public async Task<IActionResult> GetAmenitiesByRoomClassId(int roomClassId)
    {
        var forbid = await EnsureStaffOwnsRoomClass(roomClassId);
        if (forbid != null) return forbid;

        var amenities = await _amenityManagementService.GetAmenitiesByRoomClassIdAsync(roomClassId);
        return _responseHandler.Success(amenities, "Amenities retrieved successfully for the room class.");
    }

    [HttpGet("{roomClassId}/images")]
    [SwaggerOperation(Summary = "Retrieve all images associated with a specific room class.")]
    public async Task<IActionResult> GetImagesForRoomClass(int roomClassId)
    {
        var forbid = await EnsureStaffOwnsRoomClass(roomClassId);
        if (forbid != null) return forbid;

        var allImages = await _imageService.GetImagesByTypeAsync("roomClass");
        var roomClassImages = allImages.Where(img => img.EntityId == roomClassId);

        if (!roomClassImages.Any())
            return _responseHandler.NotFound("No images found for the specified room class.");

        return _responseHandler.Success(roomClassImages, "Images retrieved successfully for the room class.");
    }
}
