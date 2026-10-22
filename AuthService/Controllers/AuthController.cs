using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(
            RegisterRequest request, 
            CancellationToken cancellationToken)
        {
            UserResponse user = await _accountService.RegisterAsync(
                request, 
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, user);
        }

        [HttpPost("login")]
        [ProducesResponseType(
            typeof(UserResponse), 
            StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(
            LoginRequest request, 
            CancellationToken cancellationToken)
        {
            var response = await _accountService.LoginAsync(
                request, 
                cancellationToken);

            return Ok(response);
        }
    }
}
