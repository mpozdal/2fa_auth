using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TwoFactorService.Application.Contracts;
using TwoFactorService.Application.Interfaces;

namespace TwoFactorService.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/2fa")]
    [Produces("application/json")]
    public class TwoFactorController(ITwoFactorService service) : ControllerBase
    {
        private readonly ITwoFactorService _service = service;

        [HttpPost("setup")]
        [ProducesResponseType(typeof(ServiceResult<SetupResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Setup()
        {
            var userId = GetUserIdFromToken();

            var setupInfo = await _service.GenerateSetupAsync(userId, "MyIssuer");

            if (!setupInfo.IsSuccess)
            {
                return UnprocessableEntity(setupInfo);
            }

            return Ok(setupInfo);
        }

        [HttpPost("enable")]
        [ProducesResponseType(typeof(ServiceResult<SetupVerificationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [EnableRateLimiting("bruteforce_2fa")]
        public async Task<IActionResult> Enable([FromBody] SetupVerificationRequest request)
        {
            var userId = GetUserIdFromToken();

            var verificationResponse = await _service.VerifyAndEnableAsync(userId, request.Code);
            if (!verificationResponse.IsSuccess)
            {
                return UnprocessableEntity(verificationResponse);
            }

            return Ok(verificationResponse);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Disable()
        {
            var userId = GetUserIdFromToken();

            var response = await _service.Disable2FAAsync(userId);
            if (!response.IsSuccess)
            {
                return UnprocessableEntity(response);
            }

            return NoContent();
        }

        private string GetUserIdFromToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            return userId;
        }
    }
}
