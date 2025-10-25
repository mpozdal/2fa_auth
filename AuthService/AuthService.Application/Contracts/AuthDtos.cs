namespace AuthService.Application.Contracts
{
    public record LoginRequest(string Email, string Password);

    public record Verify2FARequest(string UserId, string Code);
}
