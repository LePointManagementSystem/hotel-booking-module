using HotelBookingPlatform.Application.Core.Abstracts.StaffManagementService;
using HotelBookingPlatform.Domain.DTOs.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.API.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [ApiController]
    [Route("api/[controller]")]

    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _staffService.GetAllAsync();
            return Ok(new { StatusCode = 200, succeded = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaffCreateRequest request)
        {
            var result = await _staffService.CreateAsync(request);
            return Created("", new { StatusCode = 201, succeeded = true, data = result });

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StaffCreateRequest request)
        {
            var result = await _staffService.UpdateAsync(id, request);
            return Ok(new { StatusCode = 200, succeeded = true, data = result });

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _staffService.DeleteAsync(id);
            return Ok(new { StatusCode = 200, succeeded = true, message = "Staff deleted successfully" });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult>ToggleStatus(int id, [FromBody] bool IsActive)
        {
            var staff = await _staffService.UpdateStatusAsync(id, IsActive);
            return Ok(new { StatusCode = 200, succeeded = true, message = "Staff status updated", data = staff });
        }
    }
}