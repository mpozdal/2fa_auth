namespace AuthService.Application.Contracts
{
    public record UserInfoResponse(string Id, string Email, bool TwoFactorEnabled);
}
