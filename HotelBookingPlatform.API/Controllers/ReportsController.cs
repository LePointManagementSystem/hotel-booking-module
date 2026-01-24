using HotelBookingPlatform.API.Reporting;
using HotelBookingPlatform.Application.Core.Abstracts;
using HotelBookingPlatform.Application.Core.Abstracts.CashManagementService;
using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
using HotelBookingPlatform.Application.Core.Abstracts.ReportsManagementService;
using HotelBookingPlatform.Application.Core.Abstracts.StaffManagementService;
using HotelBookingPlatform.Domain.DTOs.HomePage;
using HotelBookingPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;
    private readonly IResponseHandler _responseHandler;

    private readonly IBookingService _bookingService;
    private readonly ICashSessionService _cashSessionService;
    private readonly ICashTransactionService _cashTransactionService;
    private readonly IGuestService _guestService;
    private readonly IStaffService _staffService;

    public ReportsController(
        IReportsService reportsService,
        IResponseHandler responseHandler,
        IBookingService bookingService,
        ICashSessionService cashSessionService,
        ICashTransactionService cashTransactionService,
        IGuestService guestService,
        IStaffService staffService
    )
    {
        _reportsService = reportsService;
        _responseHandler = responseHandler;
        _bookingService = bookingService;
        _cashSessionService = cashSessionService;
        _cashTransactionService = cashTransactionService;
        _guestService = guestService;
        _staffService = staffService;
    }

    // =========================
    // Monthly report (JSON)
    // GET /api/Reports/monthly-hotel?hotelId=1&month=1&year=2026
    // =========================
    [HttpGet("monthly-hotel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetMonthlyHotelReport([FromQuery] int hotelId, [FromQuery] int month, [FromQuery] int year)
    {
        var report = await _reportsService.GetMonthlyHotelReportAsync(hotelId, year, month);
        return _responseHandler.Success(report, "Monthly hotel report retrieved successfully");
    }

    // =========================
    // Helpers
    // =========================
    private int? GetScopedHotelId()
    {
        var hotelIdStr = User.FindFirst("hotelId")?.Value;
        return int.TryParse(hotelIdStr, out var id) ? id : null;
    }

    private static string DateStamp(DateTime? fromUtc, DateTime? toUtc)
    {
        string f = fromUtc.HasValue ? fromUtc.Value.ToString("yyyyMMdd") : "ALL";
        string t = toUtc.HasValue ? toUtc.Value.ToString("yyyyMMdd") : "ALL";
        return $"{f}-{t}";
    }

    private bool IsStaff() => User.IsInRole("Staff");

    private IActionResult? ForbidHotelScopeIfNeeded(int? requestedHotelId)
    {
        var scoped = GetScopedHotelId();
        if (IsStaff())
        {
            if (!scoped.HasValue) return Forbid();
            if (requestedHotelId.HasValue && requestedHotelId.Value != scoped.Value) return Forbid();
        }
        return null;
    }

    // ============================================================
    // EXPORT: BOOKINGS
    // GET /api/Reports/export/bookings?hotelId=1&fromUtc=...&toUtc=...&status=confirmed
    // ============================================================
    [HttpGet("export/bookings")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> ExportBookings(
        [FromQuery] int? hotelId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] string? status
    )
    {
        var forbid = ForbidHotelScopeIfNeeded(hotelId);
        if (forbid != null) return forbid;

        var scopedHotelId = GetScopedHotelId();
        var effectiveHotelId = IsStaff() ? scopedHotelId : hotelId;

        var all = await _bookingService.GetAllBookingsAsync();
        var filtered = all.AsEnumerable();

        if (effectiveHotelId.HasValue)
            filtered = filtered.Where(b => b.HotelId == effectiveHotelId.Value);

        // overlap window
        if (fromUtc.HasValue)
            filtered = filtered.Where(b => b.CheckOutDateUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            filtered = filtered.Where(b => b.CheckInDateUtc <= toUtc.Value);

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var s = status.Trim().ToLowerInvariant();
            filtered = filtered.Where(b => (b.Status ?? "").ToLowerInvariant().Contains(s));
        }

        var list = filtered
            .OrderByDescending(b => b.BookingDateUtc)
            .ToList();

        var bytes = ExcelReportWriter.Build("Bookings", ws =>
        {
            ExcelReportWriter.WriteHeader(ws, 1,
                "BookingId",
                "BookingRef",
                "HotelId",
                "Guest",
                "Rooms",
                "Status",
                "PaymentMethod",
                "BookingDateUtc",
                "CheckInDateUtc",
                "CheckOutDateUtc",
                "BookedBy"
            );

            int r = 2;
            foreach (var b in list)
            {
                ExcelReportWriter.WriteRow(ws, r++,
                    b.BookingId,
                    b.ConfirmationNumber,
                    b.HotelId,
                    $"{b.GuestFirstName} {b.GuestLastName}".Trim(),
                    b.Numbers != null ? string.Join(", ", b.Numbers) : "",
                    b.Status,
                    b.PaymentMethod,
                    b.BookingDateUtc,
                    b.CheckInDateUtc,
                    b.CheckOutDateUtc,
                    b.UserName
                );
            }
        });

        var fileName =
            $"Bookings_{(effectiveHotelId.HasValue ? $"Hotel{effectiveHotelId.Value}_" : "")}{DateStamp(fromUtc, toUtc)}.xlsx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // EXPORT: CASH SESSIONS
    // GET /api/Reports/export/cash-sessions?hotelId=1&fromUtc=...&toUtc=...
    // ============================================================
    [HttpGet("export/cash-sessions")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> ExportCashSessions(
        [FromQuery] int? hotelId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc
    )
    {
        var forbid = ForbidHotelScopeIfNeeded(hotelId);
        if (forbid != null) return forbid;

        var scopedHotelId = GetScopedHotelId();
        var effectiveHotelId = IsStaff() ? scopedHotelId : hotelId;

        if (!effectiveHotelId.HasValue)
            return _responseHandler.BadRequest("hotelId is required for export.");

        // ✅ IMPORTANT: chez toi ça retourne IReadOnlyList<CashSessionDto>
        var list = await _cashSessionService.GetByHotelAsync(
            effectiveHotelId.Value,
            fromUtc,
            toUtc,
            currency: null,
            shift: null,
            page: 1,
            pageSize: 5000,
            scopedHotelId: scopedHotelId,
            isStaff: IsStaff()
        );

        var bytes = ExcelReportWriter.Build("CashSessions", ws =>
        {
            ExcelReportWriter.WriteHeader(ws, 1,
                "CashSessionId",
                "HotelId",
                "Currency",
                "Shift",
                "OpeningBalance",
                "OpenedAtUtc",
                "Expected",
                "ClosingCounted",
                "ClosedAtUtc",
                "Difference",
                "IsClosed"
            );

            int r = 2;
            foreach (var s in list)
            {
                ExcelReportWriter.WriteRow(ws, r++,
                    s.CashSessionId,
                    s.HotelId,
                    s.Currency.ToString(),
                    s.Shift.ToString(),
                    s.OpeningBalance,
                    s.OpenedAtUtc,
                    s.Expected,
                    s.ClosingCounted,
                    s.ClosedAtUtc,
                    s.Difference,
                    s.IsClosed
                );
            }
        });

        var fileName = $"CashSessions_Hotel{effectiveHotelId.Value}_{DateStamp(fromUtc, toUtc)}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // EXPORT: CASH TRANSACTIONS (Petty Cash)
    // GET /api/Reports/export/cash-transactions?hotelId=1&fromUtc=...&toUtc=...
    // ============================================================
    [HttpGet("export/cash-transactions")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> ExportCashTransactions(
        [FromQuery] int? hotelId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] CurrencyCode? currency,
        [FromQuery] CashShift? shift,
        [FromQuery] int? type
    )
    {
        var forbid = ForbidHotelScopeIfNeeded(hotelId);
        if (forbid != null) return forbid;

        var scopedHotelId = GetScopedHotelId();
        var effectiveHotelId = IsStaff() ? scopedHotelId : hotelId;

        if (!effectiveHotelId.HasValue)
            return _responseHandler.BadRequest("hotelId is required for export.");

        var txType = type.HasValue && (type == 1 || type == 2)
            ? (CashTransactionType?)type.Value
            : null;

        var list = await _cashTransactionService.GetByHotelAsync(
            effectiveHotelId.Value,
            fromUtc,
            toUtc,
            txType,
            currency,
            shift,
            page: 1,
            pageSize: 10000,
            scopedHotelId: scopedHotelId,
            isStaff: IsStaff()
        );

        var bytes = ExcelReportWriter.Build("PettyCash", ws =>
        {
            ExcelReportWriter.WriteHeader(ws, 1,
                "CashTransactionId",
                "HotelId",
                "CashSessionId",
                "CreatedAtUtc",
                "Shift",
                "Type",
                "Amount",
                "Currency",
                "Category",
                "Note",
                "Reference",
                "ActorUserName"
            );

            int r = 2;
            foreach (var t in list)
            {
                ExcelReportWriter.WriteRow(ws, r++,
                    t.CashTransactionId,
                    t.HotelId,
                    t.CashSessionId,
                    t.CreatedAtUtc,
                    t.Shift.ToString(),
                    t.Type.ToString(),
                    t.Amount,
                    t.Currency.ToString(),
                    t.Category,
                    t.Note,
                    t.Reference,
                    t.ActorUserName ?? t.ActorUserId
                );
            }
        });

        var fileName = $"PettyCash_Hotel{effectiveHotelId.Value}_{DateStamp(fromUtc, toUtc)}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // EXPORT: GUESTS
    // ✅ ton GuestResponse n'a pas PhoneNumber/NIF => on exporte les champs existants
    // ============================================================
    [HttpGet("export/guests")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ExportGuests()
    {
        var list = await _guestService.GetAllGuestsAsync();

        var bytes = ExcelReportWriter.Build("Guests", ws =>
        {
            ExcelReportWriter.WriteHeader(ws, 1,
                "GuestId",
                "FirstName",
                "LastName",
                "CIN",
                "Email"
            );

            int r = 2;
            foreach (var g in list)
            {
                ExcelReportWriter.WriteRow(ws, r++,
                    g.Id,
                    g.FirstName,
                    g.LastName,
                    g.CIN,
                    g.Email
                );
            }
        });

        var fileName = $"Guests_{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ============================================================
    // EXPORT: STAFF
    // ✅ StaffResponseDto => StaffId (pas Id)
    // ============================================================
    [HttpGet("export/staff")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ExportStaff()
    {
        var list = await _staffService.GetAllAsync();

        var bytes = ExcelReportWriter.Build("Staff", ws =>
        {
            ExcelReportWriter.WriteHeader(ws, 1,
                "StaffId",
                "FirstName",
                "LastName",
                "Email",
                "PhoneNumber",
                "IsActive",
                "HotelId",
                "Role",
                "CreatedAtUtc",
                "UpdatedAtUtc"
            );

            int r = 2;
            foreach (var s in list)
            {
                ExcelReportWriter.WriteRow(ws, r++,
                    s.StaffId,
                    s.FirstName,
                    s.LastName,
                    s.Email,
                    s.PhoneNumber,
                    s.IsActive,
                    s.HotelId,
                    s.Role,
                    s.CreatedAtUtc,
                    s.UpdatedAtUtc
                );
            }
        });

        var fileName = $"Staff_{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
