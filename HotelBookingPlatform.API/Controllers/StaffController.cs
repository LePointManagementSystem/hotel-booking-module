using HotelBookingPlatform.Application.Core.Abstracts.StaffManagementService;
using HotelBookingPlatform.Domain.DTOs.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly IResponseHandler _responseHandler;

        public StaffController(IStaffService staffService, IResponseHandler responseHandler)
        {
            _staffService = staffService;
            _responseHandler = responseHandler;
        }

        // ✅ Robust user id getter (supporte plusieurs tokens / claim types)
        private string? GetUserId()
        {
            // 1) Standard .NET Identity
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(id)) return id;

            // 2) JWT common
            id = User.FindFirstValue("sub");
            if (!string.IsNullOrWhiteSpace(id)) return id;

            // 3) Some providers put it in Name
            id = User.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrWhiteSpace(id)) return id;

            return null;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _staffService.GetAllAsync();
            return Ok(new { StatusCode = 200, succeded = true, data = result });
        }

        [HttpGet("me")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetMyStaffProfile()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "UserId claim not found in token." });

            var staff = await _staffService.GetByUserIdAsync(userId);

            if (staff == null)
                return NotFound(new { message = "No staff profile linked to this user." });

            return _responseHandler.Success(staff, "Staff profile loaded successfully");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] StaffCreateRequest request)
        {
            var result = await _staffService.CreateAsync(request);
            return Created("", new { StatusCode = 201, succeeded = true, data = result });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] StaffCreateRequest request)
        {
            var result = await _staffService.UpdateAsync(id, request);
            return Ok(new { StatusCode = 200, succeeded = true, data = result });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            await _staffService.DeleteAsync(id);
            return Ok(new { StatusCode = 200, succeeded = true, message = "Staff deleted successfully" });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool IsActive)
        {
            var staff = await _staffService.UpdateStatusAsync(id, IsActive);
            return Ok(new { StatusCode = 200, succeeded = true, message = "Staff status updated", data = staff });
        }

        [HttpPost("{id}/attach-user")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AttachUserToStaff(int id, [FromBody] AttachUserToStaffRequest request)
        {
            var result = await _staffService.AttachUserAsync(id, request.Email);
            return Ok(new
            {
                StatusCode = 200,
                succeeded = true,
                message = "User attached to staff successfully",
                data = result
            });
        }
    }
}
