using Microsoft.EntityFrameworkCore;
using TwoFactorService.Domain.Entities;

namespace TwoFactorService.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<UserTwoFactorSetting> UserTwoFactorSettings { get; }
        DbSet<UserRecoveryCode> UserRecoveryCodes { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
