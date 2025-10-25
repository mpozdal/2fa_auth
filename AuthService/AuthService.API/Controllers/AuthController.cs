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
           await _authService.Login(request.Email, request.Password);

            return Ok("loginResponse");
        }
    }
}
