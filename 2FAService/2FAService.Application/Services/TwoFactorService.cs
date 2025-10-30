using Google.Authenticator;
using Microsoft.AspNetCore.DataProtection;
using TwoFactorService.Application.Contracts;
using TwoFactorService.Application.Helpers;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Domain.Entities;
using MassTransit;
using Shared.Events;

namespace TwoFactorService.Application.Services
{
    public class TwoFactorAuthService(IUnitOfWork unitOfWork, IDataProtectionProvider provider, IPublishEndpoint publishEndpoint) : ITwoFactorService
    {

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDataProtector _protector = provider.CreateProtector("TwoFactorService.v1.Secrets");
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;


        public async Task<ServiceResult<SetupResponse>> GenerateSetupAsync(string userId, string issuerName)
        {
            var exisitingSetting = await _unitOfWork.Settings.GetByUserIdAsync(userId);

            if (exisitingSetting != null && exisitingSetting.IsEnabled)
            {
                return ServiceResult<SetupResponse>.Fail(TwoFactorErrorCodes.AlreadyEnabled);
            }

            var tfa = new TwoFactorAuthenticator();

            var secretKey = SecretKeyGenerator.Generate();
            var encryptedSecret = _protector.Protect(secretKey);

            if (exisitingSetting is null)
            {
                exisitingSetting = new UserTwoFactorSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EncryptedSecretKey = encryptedSecret,
                    IsEnabled = false
                };
                await _unitOfWork.Settings.AddAsync(exisitingSetting);
            }
            else
            {
                exisitingSetting.EncryptedSecretKey = encryptedSecret;
                exisitingSetting.IsEnabled = false;

                _unitOfWork.Settings.Update(exisitingSetting);
            }

            await _unitOfWork.CompleteAsync();

            SetupCode setupInfo = tfa.GenerateSetupCode(issuerName, userId, secretKey, false);

            string qrCodeBase64 = CodeQRGenerator.GenerateQrCodeAsBase64(setupInfo.QrCodeSetupImageUrl);

            return ServiceResult<SetupResponse>.Success(new SetupResponse(userId, setupInfo.ManualEntryKey, qrCodeBase64));
        }

        public async Task<ServiceResult<SetupVerificationResponse>> VerifyAndEnableAsync(string userId, string code)
        {
            var userSetting = await _unitOfWork.Settings.GetByUserIdAsync(userId);

            if (userSetting is null || userSetting.IsEnabled)
            {
               return ServiceResult<SetupVerificationResponse>.Fail(TwoFactorErrorCodes.AlreadyEnabled);
            }

            var tfa = new TwoFactorAuthenticator();
            string secretKey;

            try
            {
                secretKey = _protector.Unprotect(userSetting.EncryptedSecretKey);
            }
            catch
            {
                return ServiceResult<SetupVerificationResponse>.Fail(TwoFactorErrorCodes.CryptoError);
            }

            var isValid = tfa.ValidateTwoFactorPIN(secretKey, code);

            if (!isValid)
            {
                return ServiceResult<SetupVerificationResponse>.Fail(TwoFactorErrorCodes.InvalidCode);
            }

            userSetting.IsEnabled = true;
            _unitOfWork.Settings.Update(userSetting);

            var plainTextRecoveryCodes = RecoverCodesGenerator.Generate(10);

            foreach (var plainCode in plainTextRecoveryCodes)
            {
                string hashCode = BCrypt.Net.BCrypt.HashPassword(plainCode);

                var recoveryCode = new UserRecoveryCode
                {
                    Id = Guid.NewGuid(),
                    HashedCode = hashCode,
                    IsUsed = false,
                    UserTwoFactorSettingId = userSetting.Id,
                    UserTwoFactorSetting = userSetting
                };

                await _unitOfWork.RecoveryCodes.AddAsync(recoveryCode);
            }

            await _unitOfWork.CompleteAsync();

            var @event = new User2FAEnabledEvent { UserId = userSetting.UserId };

            await _publishEndpoint.Publish(@event);

            return ServiceResult<SetupVerificationResponse>.Success(new SetupVerificationResponse(plainTextRecoveryCodes));
        }

        public async Task<ServiceResult> VerifyLoginAsync(string userId, string code)
        {
            var userSetting = await _unitOfWork.Settings.GetByUserIdAsync(userId);
            if (userSetting is null || !userSetting.IsEnabled)
            {
                return ServiceResult.Fail(TwoFactorErrorCodes.NotEnabled);
            }

            var tfa = new TwoFactorAuthenticator();
            string secretKey;

            try
            {
                 secretKey = _protector.Unprotect(userSetting.EncryptedSecretKey);
            }
            catch
            {
                return ServiceResult<SetupVerificationResponse>.Fail(TwoFactorErrorCodes.CryptoError);
            }

            var isValid = tfa.ValidateTwoFactorPIN(secretKey, code);

            if (!isValid)
            {
                var recoveryCodeUsed = false;
                foreach (var recoveryCode in userSetting.RecoveryCodes.Where(rc => !rc.IsUsed))
                {
                    if (BCrypt.Net.BCrypt.Verify(code, recoveryCode.HashedCode))
                    {
                        recoveryCode.IsUsed = true;
                        _unitOfWork.RecoveryCodes.Update(recoveryCode);
                        await _unitOfWork.CompleteAsync();
                        recoveryCodeUsed = true;
                        break;
                    }
                }
                if (!recoveryCodeUsed)
                {
                    return ServiceResult.Fail(TwoFactorErrorCodes.InvalidCode);
                }
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> Disable2FAAsync(string userId)
        {
            var userSetting = await _unitOfWork.Settings.GetByUserIdAsync(userId);

            if (userSetting is null || !userSetting.IsEnabled)
            {
                return ServiceResult.Fail(TwoFactorErrorCodes.NotEnabled);
            }

            _unitOfWork.Settings.Delete(userSetting);

            await _unitOfWork.CompleteAsync();

            var @event = new User2FAEnabledEvent { UserId = userSetting.UserId };

            await _publishEndpoint.Publish(@event);

            return ServiceResult.Success();
        }
    }
}
