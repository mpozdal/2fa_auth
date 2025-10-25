using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TwoFactorService.Application.Contracts;
using TwoFactorService.Application.Interfaces;

namespace TwoFactorService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/2fa")]
    [Produces("application/json")]
    public class TwoFactorController(ITwoFactorService service) : ControllerBase
    {
        private readonly ITwoFactorService _service = service;

        [HttpPost("setup")]
        [ProducesResponseType(typeof(ServiceResult<SetupResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Setup([FromBody] SetupRequest request)
        {
            var setupInfo = await _service.GenerateSetupAsync(request.UserId, "MyIssuer");

            if (!setupInfo.IsSuccess)
            {
                return UnprocessableEntity(setupInfo.ErrorCode);
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
            var verificationResponse = await _service.VerifyAndEnableAsync(request.UserId, request.Code);
            if (!verificationResponse.IsSuccess)
            {
                return UnprocessableEntity(verificationResponse.ErrorCode);
            }

            return Ok(verificationResponse);
        }

        [HttpPost("verify-login")]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [EnableRateLimiting("bruteforce_2fa")]
        public async Task<IActionResult> Verify([FromBody] VerifyCodeRequest request)
        {
            var verificationResponse = await _service.VerifyLoginAsync(request.UserId, request.Code);
            if (!verificationResponse.IsSuccess)
            {
                return UnprocessableEntity(verificationResponse.ErrorCode);
            }

            return Ok(verificationResponse);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Disable([FromBody] Disable2FARequest request)
        {
            var response = await _service.Disable2FAAsync(request.UserId);
            if (!response.IsSuccess)
            {
                return UnprocessableEntity(response.ErrorCode);
            }

            return NoContent();
        }

    }
}
