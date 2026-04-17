using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
using Microsoft.AspNetCore.Http.HttpResults;
namespace HotelBookingPlatform.API.Controllers;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/[controller]")]
[ApiController]
[ResponseCache(CacheProfileName = "DefaultCache")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IResponseHandler _responseHandler;
    private readonly IEmailService _emailService;
    private readonly ILog _log;
    public BookingController(IBookingService bookingService, IResponseHandler responseHandler, IEmailService emailService, ILog log)
    {
        _bookingService = bookingService;
        _responseHandler = responseHandler;
        _emailService = emailService;
        _log = log;
    }

    private int? GetScopedHotelId()
    {
        var hotelIdStr = User.FindFirst("hotelId")?.Value;
        return int.TryParse(hotelIdStr, out var id) ? id : null;
    }

    private string? GetUserEmail()
    {
        return User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value;
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [SwaggerOperation(
        Summary = "Retrieve all bookings",
        Description = "Fetch all bookings from the system",
        OperationId = "GetAllBookings",
        Tags = new[] { "Booking" })]
    public async Task<IActionResult> GetAllBookings()
    {
        var scopedHotelId = GetScopedHotelId();
        
        if (User.IsInRole("Staff") && scopedHotelId.HasValue)
        {
            var scoped = await _bookingService.GetBookingsByHotelAsync(scopedHotelId.Value);
            return _responseHandler.Success(scoped);

        }
        

        var result = await _bookingService.GetAllBookingsAsync();
        return _responseHandler.Success(result);

    }

    [HttpPost("confirm")]
    [SwaggerOperation(Summary = "Send a confirmation email", Description = "This endpoint sends a confirmation email to the specified user. The email contains booking confirmation details.",
                      OperationId = "ConfirmBooking", Tags = new[] { "Booking" })]
    public async Task<IActionResult> ConfirmBooking([FromBody] BookingConfirmation confirmation)
    {
        if (confirmation is null || string.IsNullOrEmpty(confirmation.UserEmail))
            return BadRequest("Confirmation data is missing or invalid.");

        await _emailService.SendConfirmationEmailAsync(confirmation);
        _log.Log($"ConfirmBooking: Confirmation email sent successfully to {confirmation.UserEmail}.", "info");
        return _responseHandler.Success("Confirmation email sent successfully.");
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [SwaggerOperation(Summary = "Retrieve a booking by its unique identifier.")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var booking = await _bookingService.GetBookingAsync(id);

        if (booking is null)
            return NotFound($"Booking with ID {id} not found.");


        var scopedHotelId = GetScopedHotelId();
        if (User.IsInRole("Staff") && scopedHotelId.HasValue && booking.HotelId != scopedHotelId.Value)
            return Forbid("You are not allowed to access bookings from another hotel.");

        _log.Log($"GetBooking: Retrieved booking with ID {id}.", "info");
        return _responseHandler.Success(booking, "Booking retrieved successfully.");
    }

    [HttpPost]
    [Route("create")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [SwaggerOperation(Summary = "Create a new booking", Description = "Creates a new booking record in the system. The request must include details of the booking such as check-in and check-out dates, room IDs, payment method, and hotel ID. The user making the request must be authenticated.",
     OperationId = "CreateBooking",
     Tags = new[] { "Booking" })]
    public async Task<IActionResult> CreateBooking([FromBody] BookingCreateRequest request)
    {
        
        var userEmail = GetUserEmail();

        if (string.IsNullOrEmpty(userEmail))
        {
            _log.Log("CreateBooking: User email not found in token.", "Warning");
            return Unauthorized("User email not found in token.");
        }

        var booking = await _bookingService.CreateBookingAsync(request, userEmail);
        return _responseHandler.Success(booking, "Booking created successfully.");

    }



[HttpPut("{id}/Update_status")]
[Authorize(Roles = "User,Admin,Manager,Staff")]
[SwaggerOperation(Summary = "Update the status of a booking.")]
public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] BookingStatus newStatus)
{
    var booking = await _bookingService.GetBookingAsync(id);

    if (booking is null)
    {
        _log.Log($"UpdateBookingStatus: Booking with ID {id} not found.", "Warning");
        return NotFound($"Booking with ID {id} not found.");
    }

    var scopedHotelId = GetScopedHotelId();
    if (User.IsInRole("Staff") && scopedHotelId.HasValue && booking.HotelId != scopedHotelId.Value)
        return Forbid("You are not allowed to update a booking from another hotel.");

    // NOTE: Cancellation is handled by the dedicated /cancel endpoint
    // because it requires a reason and stores audit fields.
    if (newStatus == BookingStatus.Cancelled)
    {
        _log.Log($"UpdateBookingStatus: Cancelled is not allowed via Update_status for booking {id}. Use /api/Booking/{id}/cancel.", "Warning");
        return BadRequest("To cancel a booking, use PUT /api/Booking/{id}/cancel with a cancellation reason.");
    }

    if (newStatus != BookingStatus.Completed &&
        newStatus != BookingStatus.Confirmed)
    {
        _log.Log($"UpdateBookingStatus: Invalid status '{newStatus}'.", "Warning");
        return BadRequest("Invalid status update request.");
    }

    await _bookingService.UpdateBookingStatusAsync(id, newStatus);

    var userEmail = GetUserEmail();
    var userName = User.Identity?.Name;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;

    _log.Log($"UpdateBookingStatus: Booking {id} status updated to '{newStatus}' by {userName} ({role}).", "info");

    return _responseHandler.Success($"Booking status updated to {newStatus} successfully");
}

public sealed class CancelBookingRequest
{
    public string Reason { get; set; } = string.Empty;
}

[HttpPut("{id}/cancel")]
[Authorize(Roles = "User,Admin,Manager,Staff")]
[SwaggerOperation(
    Summary = "Cancel booking with reason",
    Description = "Sets booking status to Cancelled and stores a cancellation reason (audit). Use this instead of Update_status.",
    Tags = new[] { "Booking" })]
public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequest request)
{
    if (id <= 0) return BadRequest("Invalid booking id.");
    if (request == null) return BadRequest("Request body is required.");
    if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest("Reason is required.");

    var cancelledByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    await _bookingService.CancelBookingAsync(id, request.Reason, cancelledByUserId);
   

    var userName = User.Identity?.Name;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    _log.Log($"CancelBooking: Booking {id} cancelled by {userName} ({role}). Reason: {request.Reason}", "info");

    return _responseHandler.Success("Booking cancelled successfully");
}


    [HttpPut("release-expired")]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = " force release of expired bookings", Description = " Mark expired bookings as completed and free up the rooms.",
    Tags = new[] { "Booking" })]
    public async Task<IActionResult> ReleaseExpiredBookings()
    {
        var result = await _bookingService.ReleaseExpiredBookingsAsync();

        if (result.Count == 0)
            return Ok("No expired bookings found at this time.");
        return Ok(result);
    }
}


