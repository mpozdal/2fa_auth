using TwoFactorService.Domain.Entities;

namespace TwoFactorService.Application.Interfaces
{
    public interface IRecoveryCodeRepository: IGenericRepository<UserRecoveryCode>
    {
        Task<UserRecoveryCode?> FindUnusedCodeAsync(Guid settingId, string hashedCode);
    }
}
