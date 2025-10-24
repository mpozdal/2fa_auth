using Microsoft.EntityFrameworkCore;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Domain.Entities;

namespace TwoFactorService.Infrastructure.Persistence.Repositories
{
    public class RecoveryCodeRepository(IAppDbContext context) : GenericRepository<UserRecoveryCode>(context), IRecoveryCodeRepository
 {
        public async Task<UserRecoveryCode?> FindUnusedCodeAsync(Guid settingId, string hashedCode)
        {
            return await _dbSet.FirstOrDefaultAsync(rc =>
                rc.UserTwoFactorSettingId == settingId &&
                rc.HashedCode == hashedCode &&
                !rc.IsUsed);
        }
    }
}
