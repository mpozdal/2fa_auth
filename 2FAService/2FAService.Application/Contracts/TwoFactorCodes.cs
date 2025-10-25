namespace TwoFactorService.Application.Contracts
{
    public static class TwoFactorErrorCodes
    {
        public const string AlreadyEnabled = "2FA_ALREADY_ENABLED";
        public const string NotEnabled = "2FA_NOT_ENABLED";
        public const string InvalidCode = "INVALID_CODE";
        public const string CryptoError = "CRYPTO_ERROR";
    }
}
