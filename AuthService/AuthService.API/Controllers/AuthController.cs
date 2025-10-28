using Microsoft.AspNetCore.Mvc;
using AuthService.Application.Interfaces;
using AuthService.Application.Contracts;
using TwoFactorService.Application.Contracts;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController(IAuthenticationService service, IConfiguration configuration) : ControllerBase
    {
        private readonly IAuthenticationService _authService = service;
        private readonly IConfiguration _configuration = configuration;
        private string RefreshTokenCookiePath => _configuration["Jwt:RefreshTokenCookiePath"]!;
        private string RefreshTokenCookieName => _configuration["Jwt:RefreshTokenCookieName"]!;

        [HttpPost("login")]
        [ProducesResponseType(typeof(ServiceResult<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

            return HandleAuthSuccess(result);
        }

        [HttpPost("verify2fa")]
        [ProducesResponseType(typeof(ServiceResult<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest request)
        {
            var result = await _authService.Verify2FAAsync(request.UserId, request.Code);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

           return HandleAuthSuccess(result);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ServiceResult<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(ServiceResult.Fail(AuthErrorCodes.InvalidRefreshToken));
            }

            var result = await _authService.RefreshAsync(refreshToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

           return HandleAuthSuccess(result);
        }

        [HttpPost("revoke")]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Revoke()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.RevokeAsync(refreshToken);
            }

            Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = RefreshTokenCookiePath,
            });

            return Ok(ServiceResult.Success());
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request.Email, request.Password);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private void SetRefreshTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = RefreshTokenCookiePath,
                Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiresInDays"]))
            };
            Response.Cookies.Append(RefreshTokenCookieName,  token, cookieOptions);
        }

        private IActionResult HandleAuthSuccess(ServiceResult<LoginResponse> result)
        {
            if (result.Value?.RefreshToken is not null)
            {
                SetRefreshTokenCookie(result.Value.RefreshToken);

                var response = ServiceResult<LoginResponse>.Success(result.Value with { RefreshToken = null });
                return Ok(response);
            }

            return Ok(result);
        }
    }
}
