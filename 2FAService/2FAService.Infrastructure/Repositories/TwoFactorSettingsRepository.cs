using Microsoft.EntityFrameworkCore;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Domain.Entities;

namespace TwoFactorService.Infrastructure.Persistence.Repositories
{
    public class TwoFactorSettingsRepository(IAppDbContext context) : GenericRepository<UserTwoFactorSetting>(context), ITwoFactorSettingsRepository
    {
        public async Task<UserTwoFactorSetting?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(s => s.RecoveryCodes)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
