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
            //return Ok(scoped);
            return _responseHandler.Success(scoped);

        }
        

        var result = await _bookingService.GetAllBookingsAsync();
        //return Ok(result);
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

    // [HttpPut("{id}/Update_status")]
    // [Authorize(Roles = "User,Admin,Manager,Staff")]
    // [SwaggerOperation(Summary = "Update the status of a booking.")]
    // public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] BookingStatus newStatus)
    // {

    //     var scopedHotelId = GetScopedHotelId();
    //     if (User.IsInRole("Staff") && scopedHotelId.HasValue && booking.HotelId != scopedHotelId.Value)
    //         return Forbid("You are not allowed to update a booking from another hotel.");

    //     var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
    //     var userName = User.Identity?.Name;
    //     var role = User.FindFirst(ClaimTypes.Role)?.Value;
    //     // if (userEmail is null)
    //     //     return Unauthorized("User email not found in token.");

    //     if (string.IsNullOrEmpty(userEmail))
    //         return Unauthorized("User email not found in token.");

    //     var booking = await _bookingService.GetBookingAsync(id);

    //     if (booking is null)
    //     {
    //         _log.Log($"UpdateBookingStatus: Booking with ID {id} not found.", "Warning");
    //         return NotFound($"Booking with ID {id} not found.");
    //     }

    //     // if (booking.UserName != userEmail.Split('@')[0])
    //     //     return Unauthorized("You are not authorized to update this booking.");

    //     if (newStatus != BookingStatus.Completed &&
    //         newStatus != BookingStatus.Confirmed &&
    //         newStatus != BookingStatus.Cancelled)
    //     {
    //         // await _bookingService.UpdateBookingStatusAsync(id, newStatus);
    //         // return _responseHandler.Success("Booking status updated to Completed successfully.");
    //         _log.Log($"UpdateBookingStatus: Invalid status '{newStatus}'Attempted by {userEmail}.", "Warning");
    //         return BadRequest("Invalid status update request.");
    //     }

    //     await _bookingService.UpdateBookingStatusAsync(id, newStatus);
    //     _log.Log($"UpdateBookingStatus: Booking {id} status updated to '{newStatus}' by {userName} ({role}).", "info");

    //     return _responseHandler.Success($"Booking status updated to {newStatus} successfully");


    // }

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

    if (newStatus != BookingStatus.Completed &&
        newStatus != BookingStatus.Confirmed &&
        newStatus != BookingStatus.Cancelled)
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


