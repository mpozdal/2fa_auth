using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TwoFactorService.Application.Contracts;
using TwoFactorService.Application.Interfaces;

namespace TwoFactorService.API.Controllers
{
    [ApiController]
    [Route("api/2fa/internal")]
    [Produces("application/json")]
    public class TwoFactorInternalController(ITwoFactorService service): ControllerBase
    {
        private readonly ITwoFactorService _service = service;

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
                return UnprocessableEntity(verificationResponse);
            }

            return Ok(verificationResponse);
        }
    }
}
