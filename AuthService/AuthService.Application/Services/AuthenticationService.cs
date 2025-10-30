using AuthService.Application.Contracts;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using TwoFactorService.Application.Contracts;

namespace AuthService.Application.Services
{
    public class AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ITwoFactorApiClient twoFactorApiClient,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork) : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;
        private readonly ITwoFactorApiClient _twoFactorApiClient = twoFactorApiClient;
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ServiceResult<LoginResponse>> Verify2FAAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return ServiceResult<LoginResponse>.Fail(AuthErrorCodes.InvalidCredentials);
            }

            var result = await _twoFactorApiClient.VerifyLoginCodeAsync(userId, code);

            if (!result.IsSuccess)
            {
                return ServiceResult<LoginResponse>.Fail(AuthErrorCodes.Invalid2FA);
            }

            var response = await GenerateTokensAndSuccessResponseAsync(user);

            await _unitOfWork.CompleteAsync();

            return ServiceResult<LoginResponse>.Success(response);
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return ServiceResult<LoginResponse>.Fail(AuthErrorCodes.InvalidCredentials);
            }

            if (user.TwoFactorEnabled)
            {
                return ServiceResult<LoginResponse>.Success(new LoginResponse(true, user.Id, null, null));
            }

            var response = await GenerateTokensAndSuccessResponseAsync(user);

            await _unitOfWork.CompleteAsync();

            return ServiceResult<LoginResponse>.Success(response);
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

            if (!result.Succeeded)
            {
                return ServiceResult.Fail(AuthErrorCodes.ErrorCreatingUser);
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<LoginResponse>> RefreshAsync(string refreshToken)
        {
            var hashedRefreshToken = _refreshTokenGenerator.HashToken(refreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedRefreshToken);

            if (storedToken is null || !storedToken.IsActive)
            {
                return ServiceResult<LoginResponse>.Fail(AuthErrorCodes.InvalidRefreshToken);
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId);

            if (user is null)
            {
                return ServiceResult<LoginResponse>.Fail(AuthErrorCodes.UserNotFound);
            }

            storedToken.IsRevoked = true;
            _refreshTokenRepository.Update(storedToken);

            var newRefreshToken = _refreshTokenGenerator.GenerateToken(user.Id);

            var plainTextRefreshToken = newRefreshToken.Token;

            newRefreshToken.Token = _refreshTokenGenerator.HashToken(plainTextRefreshToken);

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            var newJsonWebToken = _jwtTokenGenerator.GenerateToken(user, await _userManager.GetRolesAsync(user));

            await _unitOfWork.CompleteAsync();

            return ServiceResult<LoginResponse>.Success(new LoginResponse(false, user.Id, newJsonWebToken, plainTextRefreshToken));
        }

        public async Task<ServiceResult> RevokeAsync(string refreshToken)
        {
            var hashedRefreshToken = _refreshTokenGenerator.HashToken(refreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedRefreshToken);

            if (storedToken is null || !storedToken.IsActive)
            {
                return ServiceResult.Fail(AuthErrorCodes.InvalidRefreshToken);
            }

            storedToken.IsRevoked = true;
            _refreshTokenRepository.Update(storedToken);

            await _unitOfWork.CompleteAsync();

            return ServiceResult.Success();
        }

        private async Task<LoginResponse> GenerateTokensAndSuccessResponseAsync(ApplicationUser user)
        {
            var jsonWebToken = _jwtTokenGenerator.GenerateToken(user, await _userManager.GetRolesAsync(user));

            _refreshTokenRepository.RemoveOldTokensForUser(user.Id);

            var refreshToken = _refreshTokenGenerator.GenerateToken(user.Id);

            var plainTextRefreshToken = refreshToken.Token;

            refreshToken.Token = _refreshTokenGenerator.HashToken(plainTextRefreshToken);

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new LoginResponse(false, user.Id, jsonWebToken, plainTextRefreshToken);
        }
    }
}
