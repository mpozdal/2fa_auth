using TwoFactorService.Application.Contracts;
using TwoFactorService.Application.Interfaces;

namespace TwoFactorService.Application.Services
{
    public class TwoFactorAuthService : ITwoFactorService
    {
        public Task<SetupResponse> GenerateSetupAsync(string userId, string issuerName)
        {
            throw new NotImplementedException();
        }
    }
}
