namespace AuthService.Application.Contracts
{
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(bool RequiresTwoFactor, string? Token);

    public record RegisterRequest(string Email, string Password);

    public record Verify2FARequest(string UserId, string Code);
}
