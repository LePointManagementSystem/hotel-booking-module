using HotelBookingPlatform.Application.Core.Abstracts.CashManagementService;
using HotelBookingPlatform.Domain.DTOs.Cash;
using HotelBookingPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CashTransactionsController : ControllerBase
{
    private readonly ICashTransactionService _cashService;
    private readonly IResponseHandler _responseHandler;
    private readonly ILog _log;

    public CashTransactionsController(
        ICashTransactionService cashService,
        IResponseHandler responseHandler,
        ILog log)
    {
        _cashService = cashService;
        _responseHandler = responseHandler;
        _log = log;
    }

    private int? GetScopedHotelId()
    {
        var hotelIdStr = User.FindFirst("hotelId")?.Value;
        return int.TryParse(hotelIdStr, out var id) ? id : null;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [SwaggerOperation(
        Summary = "Create a petty cash transaction (IN/OUT)",
        Description = "Creates a cash journal entry. For Staff users, HotelId is enforced from the token scope. For OUT, Note is required.",
        Tags = new[] { "Cash" })]
    public async Task<IActionResult> Create([FromBody] CreateCashTransactionRequest request)
    {
        var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var scopedHotelId = GetScopedHotelId();
        var isStaff = User.IsInRole("Staff");

        // ✅ Backward compatible: if client didn't send Shift, service should default to Morning.
        // (If your DTO already has Shift with default, this is fine.)
        var dto = await _cashService.CreateAsync(request, actorUserId, scopedHotelId, isStaff);

        _log.Log(
            $"CashTransactions.Create: {dto.Type} {dto.Amount} {dto.Currency} shift={dto.Shift} hotel={dto.HotelId} by={actorUserId}",
            "info"
        );

        return _responseHandler.Success(dto, "Cash transaction created successfully.");
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [SwaggerOperation(
        Summary = "List cash transactions for a hotel",
        Description = "Returns petty cash journal entries with optional filters. For Staff users, hotel scope is enforced.",
        Tags = new[] { "Cash" })]
    public async Task<IActionResult> GetByHotel(
        [FromQuery] int hotelId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] CashTransactionType? type,
        [FromQuery] CurrencyCode? currency,
        [FromQuery] CashShift? shift,              // ✅ NEW FILTER
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var scopedHotelId = GetScopedHotelId();
        var isStaff = User.IsInRole("Staff");

        var list = await _cashService.GetByHotelAsync(
            hotelId,
            fromUtc,
            toUtc,
            type,
            currency,
            shift,        // ✅ pass shift to service
            page,
            pageSize,
            scopedHotelId,
            isStaff
        );

        return _responseHandler.Success(list, "Cash transactions retrieved successfully.");
    }
}
