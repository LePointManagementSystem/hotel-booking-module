// HotelBookingPlatform.API/Controllers/NotificationController.cs
using HotelBookingPlatform.Application.Core.Abstracts.NotificationManagementService;
using HotelBookingPlatform.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IResponseHandler _responseHandler;

        public NotificationController(INotificationService notificationService, IResponseHandler responseHandler)
        {
            _notificationService = notificationService;
            _responseHandler = responseHandler;
        }

        private string? GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(id)) return id;

            id = User.FindFirstValue("sub");
            if (!string.IsNullOrWhiteSpace(id)) return id;

            id = User.FindFirstValue("id");
            if (!string.IsNullOrWhiteSpace(id)) return id;

            return null;
        }

        private bool IsAdminOrManager()
            => User.IsInRole("Admin") || User.IsInRole("Manager");

        private int? GetHotelIdFromClaim()
        {
            var claim = User.FindFirstValue("hotelId");
            return int.TryParse(claim, out var hid) ? hid : null;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] bool includeRead = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int? hotelId = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.Unauthorized("User not identified.");

            if (IsAdminOrManager())
            {
                // Admin/Manager: hotelId is optional. If null => all hotels.
                var items = await _notificationService.GetForAdminAsync(hotelId, includeRead, page, pageSize);
                return _responseHandler.Success(items);
            }

            // Staff: always enforce hotel scope from token (ignore query hotelId)
            var scopedHotelId = GetHotelIdFromClaim();
            if (!scopedHotelId.HasValue)
                return _responseHandler.BadRequest("HotelId not found in token.");

            var scopedItems = await _notificationService.GetForCurrentUserAsync(
                userId, scopedHotelId.Value, includeRead, page, pageSize);
            return _responseHandler.Success(scopedItems);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount([FromQuery] int? hotelId = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.Unauthorized("User not identified.");

            if (IsAdminOrManager())
            {
                var count = await _notificationService.GetUnreadCountForAdminAsync(hotelId);
                return _responseHandler.Success(new { unread = count });
            }

            var scopedHotelId = GetHotelIdFromClaim();
            if (!scopedHotelId.HasValue)
                return _responseHandler.BadRequest("HotelId not found in token.");

            var scopedCount = await _notificationService.GetUnreadCountAsync(userId, scopedHotelId.Value);
            return _responseHandler.Success(new { unread = scopedCount });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] int id, [FromQuery] int? hotelId = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.Unauthorized("User not identified.");

            if (IsAdminOrManager())
            {
                await _notificationService.MarkAsReadAdminAsync(id);
                return _responseHandler.Success("OK");
            }

            var scopedHotelId = GetHotelIdFromClaim();
            if (!scopedHotelId.HasValue)
                return _responseHandler.BadRequest("HotelId not found in token.");

            await _notificationService.MarkAsReadAsync(id, userId, scopedHotelId.Value);
            return _responseHandler.Success("OK");
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead([FromQuery] int? hotelId = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.Unauthorized("User not identified.");

            if (IsAdminOrManager())
            {
                await _notificationService.MarkAllAsReadAdminAsync(hotelId);
                return _responseHandler.Success("OK");
            }

            var scopedHotelId = GetHotelIdFromClaim();
            if (!scopedHotelId.HasValue)
                return _responseHandler.BadRequest("HotelId not found in token.");

            await _notificationService.MarkAllAsReadAsync(userId, scopedHotelId.Value);
            return _responseHandler.Success("OK");
        }
    }
}
