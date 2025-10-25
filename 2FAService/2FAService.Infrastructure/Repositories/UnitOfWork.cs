using Microsoft.EntityFrameworkCore;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Infrastructure.Persistence.Repositories;

namespace TwoFactorService.Infrastructure.Persistence
{
    public class UnitOfWork(
        IAppDbContext context,
        ITwoFactorSettingsRepository settingsRepository,
        IRecoveryCodeRepository recoveryCodeRepository) : IUnitOfWork
    {
        private readonly IAppDbContext _context = context;

        public ITwoFactorSettingsRepository Settings { get; } = settingsRepository;
        public IRecoveryCodeRepository RecoveryCodes { get; } = recoveryCodeRepository;

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await (_context as DbContext)!.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            (_context as DbContext)?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
