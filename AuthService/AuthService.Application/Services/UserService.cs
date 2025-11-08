using AuthService.Application.Contracts;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using TwoFactorService.Application.Contracts;

namespace AuthService.Application.Services
{
    public class UserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<ServiceResult<UserInfoResponse>> GetUserInfoAsync(string userId)
        {
            var userResult = await _userManager.FindByIdAsync(userId);

            if (userResult is null)
            {
                return ServiceResult<UserInfoResponse>.Fail(AuthErrorCodes.UserNotFound);
            }

            var userInfo = new UserInfoResponse(userResult.Id, userResult.Email ?? string.Empty, userResult.TwoFactorEnabled);

            return ServiceResult<UserInfoResponse>.Success(userInfo);
        }
    }
}
