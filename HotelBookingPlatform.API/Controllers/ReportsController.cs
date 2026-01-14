using HotelBookingPlatform.Application.Core.Abstracts.ReportsManagementService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Manager,Staff")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportsService _reports;
    private readonly IResponseHandler _responseHandler;

    public ReportsController(IReportsService reports, IResponseHandler responseHandler)
    {
        _reports = reports;
        _responseHandler = responseHandler;
    }

    private int? GetScopedHotelId()
    {
        var hotelIdStr = User.FindFirst("hotelId")?.Value;
        return int.TryParse(hotelIdStr, out var id) ? id : null;
    }

    /// <summary>
    /// Monthly report (KPIs + occupancy + revenue + petty cash) for a hotel.
    /// </summary>
    [HttpGet("monthly")]
    [SwaggerOperation(
        Summary = "Monthly hotel report",
        Description = "Returns a monthly report for the selected hotel (bookings KPIs, revenue, occupancy and petty cash summary). Staff users are scoped to their hotelId claim.",
        OperationId = "GetMonthlyHotelReport",
        Tags = new[] { "Reports" })]
    public async Task<IActionResult> GetMonthly([FromQuery] int? hotelId, [FromQuery] int year, [FromQuery] int month)
    {
        var scopedHotelId = GetScopedHotelId();

        // Staff must stay in scope
        if (User.IsInRole("Staff"))
        {
            if (!scopedHotelId.HasValue)
                return _responseHandler.BadRequest("No hotel scope found for this Staff account.");

            hotelId = scopedHotelId.Value;
        }

        if (!hotelId.HasValue || hotelId.Value <= 0)
            return _responseHandler.BadRequest("hotelId is required.");

        var dto = await _reports.GetMonthlyHotelReportAsync(hotelId.Value, year, month);
        return _responseHandler.Success(dto);
    }
}
