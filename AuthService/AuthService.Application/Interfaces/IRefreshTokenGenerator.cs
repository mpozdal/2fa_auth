using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IRefreshTokenGenerator
    {
        RefreshToken GenerateToken(string userId);
    }
}
