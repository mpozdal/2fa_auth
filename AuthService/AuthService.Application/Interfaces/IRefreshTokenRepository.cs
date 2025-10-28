using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task AddAsync(RefreshToken token);
        void Update(RefreshToken token);
        void RemoveOldTokensForUser(string userId);
    }
}
