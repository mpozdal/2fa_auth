namespace TwoFactorService.Application.Contracts
{
    public record SetupRequest(string UserId);
    public record SetupResponse(
        string UserId,
        string ManualEntryKey,
        string QrCodeImageUrl
    );

    public record SetupVerificationRequest(string UserId, string Code);
    public record SetupVerificationResponse(List<string>? RecoveryCodes);

    public record VerifyCodeRequest(string UserId, string Code);

    public record Disable2FARequest(string UserId);
}

