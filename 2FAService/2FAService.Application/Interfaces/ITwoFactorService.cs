using TwoFactorService.Application.Contracts;

namespace TwoFactorService.Application.Interfaces
{
    public interface ITwoFactorService
    {
        Task<SetupResponse> GenerateSetupAsync(string userId, string issuerName);
    }
}


