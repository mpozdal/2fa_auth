using TwoFactorService.Domain.Entities;

namespace TwoFactorService.Application.Interfaces
{
    public interface ITwoFactorSettingsRepository: IGenericRepository<UserTwoFactorSetting>
    {
        Task<UserTwoFactorSetting?> GetByUserIdAsync(string userId);
    }
}
