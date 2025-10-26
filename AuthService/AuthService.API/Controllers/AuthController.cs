using Microsoft.AspNetCore.Mvc;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Application.Contracts;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController(IAuthenticationService service) : ControllerBase
    {
        private readonly IAuthenticationService _authService = service;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("verify2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest request)
        {
            var result = await _authService.Verify2FAAsync(request.UserId, request.Code);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request.Email, request.Password);

           if(!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
