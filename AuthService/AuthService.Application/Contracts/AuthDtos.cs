namespace AuthService.Application.Contracts
{
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(bool RequiresTwoFactor, string? UserId, string? Token, string? RefreshToken);

    public record RegisterRequest(string Email, string Password);

    public record Verify2FARequest(string UserId, string Code);

    public record RefreshTokenRequest(string RefreshToken);
    public record RevokeRequest(string RefreshToken);
}
