using Microsoft.EntityFrameworkCore;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Infrastructure.Persistence.Repositories;

namespace TwoFactorService.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IAppDbContext _context;

        public ITwoFactorSettingsRepository Settings { get; }
        public IRecoveryCodeRepository RecoveryCodes { get; }

        public UnitOfWork(IAppDbContext context)
        {
            _context = context;

            Settings = new TwoFactorSettingsRepository(_context);
            RecoveryCodes = new RecoveryCodeRepository(_context);
        }

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
