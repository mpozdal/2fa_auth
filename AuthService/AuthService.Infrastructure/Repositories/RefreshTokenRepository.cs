using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class RefreshTokenRespository(AuthDbContext context) : IRefreshTokenRepository
    {
        public readonly AuthDbContext _context = context;

        public async Task AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public void RemoveOldTokensForUser(string userId)
        {
            var oldTokens = _context.RefreshTokens
                .Where(rt => rt.UserId == userId && (rt.ExpiryDate <= DateTime.UtcNow || rt.IsRevoked));

            _context.RefreshTokens.RemoveRange(oldTokens);
        }

        public void Update(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
        }
    }
}
