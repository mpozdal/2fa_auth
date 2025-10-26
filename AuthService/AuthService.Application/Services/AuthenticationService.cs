using AuthService.Application.Contracts;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using TwoFactorService.Application.Contracts;

namespace AuthService.Application.Services
{
    public class AuthenticationService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator, ITwoFactorApiClient twoFactorApiClient) : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        private readonly ITwoFactorApiClient _twoFactorApiClient = twoFactorApiClient;


        public async Task<ServiceResult> Verify2FAAsync(string userId, string code)
        {
            var user = await _userManager.FindByEmailAsync(userId);

            if (user is null)
            {
                return ServiceResult.Fail(AuthErrorCodes.InvalidCredentials);
            }

            var result = await _twoFactorApiClient.VerifyLoginCodeAsync(userId, code);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(AuthErrorCodes.Invalid2FA);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var jsonWebToken = _jwtTokenGenerator.GenerateToken(user, roles);

            return ServiceResult<LoginResponse>.Success(new LoginResponse(false, jsonWebToken));
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return ServiceResult<LoginResponse>.Fail(AuthErrorCodes.InvalidCredentials);
            }

            if(user.TwoFactorEnabled)
            {
                return ServiceResult<LoginResponse>.Success(new LoginResponse(true, null));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var jsonWebToken = _jwtTokenGenerator.GenerateToken(user, roles);

            return ServiceResult<LoginResponse>.Success(new LoginResponse(false, jsonWebToken));
        }

        public async Task<ServiceResult> RegisterAsync(string email, string password)
        {
            var userExists = await _userManager.FindByEmailAsync(email);

            if (userExists is not null)
            {
                return ServiceResult.Fail(AuthErrorCodes.UserExists);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result is null)
            {
                return ServiceResult.Fail(AuthErrorCodes.ErrorCreatingUser);
            }

            return ServiceResult.Success();
        }
    }
}
