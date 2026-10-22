using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("doctors")]
        [ProducesResponseType(
            typeof(UserResponse), 
            StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateDoctor(
            RegisterRequest request, 
            CancellationToken cancellationToken)
        {
            var doctor = await _adminService.CreateDoctorAsync(
                request, 
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, doctor);
        }


    }
}
