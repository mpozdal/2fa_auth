using AuthService.Application.Contracts;
using TwoFactorService.Application.Contracts;

namespace AuthService.Application.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<UserInfoResponse>> GetUserInfoAsync(string userId);
    }
}
