using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;

namespace AuthService.Infrastructure.Data
{
    public class UnitOfWork(AuthDbContext context) : IUnitOfWork
    {
        private readonly AuthDbContext _context = context;

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
