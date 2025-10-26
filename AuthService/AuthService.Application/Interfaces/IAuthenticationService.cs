using AuthService.Application.Contracts;
using TwoFactorService.Application.Contracts;

namespace AuthService.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<ServiceResult<LoginResponse>> LoginAsync(string email, string password);
        Task<ServiceResult> RegisterAsync(string email, string password);
        Task<ServiceResult> Verify2FAAsync(string userId, string code);
    }
}
