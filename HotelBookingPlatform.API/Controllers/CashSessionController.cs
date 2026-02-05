using HotelBookingPlatform.Application.Core.Abstracts.CashManagementService;
using HotelBookingPlatform.Domain.DTOs.Cash;
using HotelBookingPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CashSessionsController : ControllerBase
{
    private readonly ICashSessionService _service;
    private readonly IResponseHandler _responseHandler;

    public CashSessionsController(ICashSessionService service, IResponseHandler responseHandler)
    {
        _service = service;
        _responseHandler = responseHandler;
    }

    private int? GetScopedHotelId()
    {
        var hotelIdStr = User.FindFirst("hotelId")?.Value;
        return int.TryParse(hotelIdStr, out var id) ? id : null;
    }

    [HttpPost("open")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> Open([FromBody] OpenCashSessionRequest request)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var scopedHotelId = GetScopedHotelId();
        var isStaff = User.IsInRole("Staff");

        var dto = await _service.OpenAsync(request, actorUserId, scopedHotelId, isStaff);
        return _responseHandler.Success(dto, "Shift opened successfully.");
    }

    // ✅ FIX ICI: {cashSessionId:int} (pas d'espace)
    [HttpPost("{cashSessionId:int}/close")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> Close(int cashSessionId, [FromBody] CloseCashSessionRequest request)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var scopedHotelId = GetScopedHotelId();
        var isStaff = User.IsInRole("Staff");

        var dto = await _service.CloseAsync(cashSessionId, request, actorUserId, scopedHotelId, isStaff);
        return _responseHandler.Success(dto, "Shift closed successfully");
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> GetByHotel(
        [FromQuery] int hotelId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] CurrencyCode? currency,
        [FromQuery] CashShift? shift,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50
    )
    {
        var scopedHotelId = GetScopedHotelId();
        var isStaff = User.IsInRole("Staff");

        var list = await _service.GetByHotelAsync(
            hotelId, fromUtc, toUtc, currency, shift, page, pageSize, scopedHotelId, isStaff
        );

        return _responseHandler.Success(list, "Cash session retrieved successfully.");
    }
}
