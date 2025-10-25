using TwoFactorService.Application.Contracts;

namespace TwoFactorService.Application.Interfaces
{
    public interface ITwoFactorService
    {
        Task<ServiceResult<SetupResponse>> GenerateSetupAsync(string userId, string issuerName);
        Task<ServiceResult<SetupVerificationResponse>> VerifyAndEnableAsync(string userId, string code);
        Task<ServiceResult> VerifyLoginAsync(string userId, string code);
        Task<ServiceResult> Disable2FAAsync(string userId);
    }
}


