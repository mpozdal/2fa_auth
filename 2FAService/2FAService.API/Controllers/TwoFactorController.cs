using Microsoft.AspNetCore.Mvc;
using TwoFactorService.Application.Contracts;
using TwoFactorService.Application.Interfaces;

namespace TwoFactorService.API.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    public class TwoFactorController(ITwoFactorService service) : ControllerBase
    {
        private readonly ITwoFactorService _service = service;

        [HttpPost("setup")]
        public async Task<IActionResult> Setup([FromBody] SetupRequest request)
        {
            var setupInfo = await _service.GenerateSetupAsync(request.UserId, "MyIssuer");

            return Ok(setupInfo);
        }

    }
}
