using TwoFactorService.Application.Contracts;

namespace AuthService.Application.Interfaces
{
    public interface ITwoFactorApiClient
    {
        Task<ServiceResult> VerifyLoginCodeAsync(string userId, string code);
    }
}
