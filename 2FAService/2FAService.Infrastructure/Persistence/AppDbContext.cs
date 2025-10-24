using Microsoft.EntityFrameworkCore;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Domain.Entities;

namespace TwoFactorService.Infrastructure.Persistance
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
    {
        public DbSet<UserTwoFactorSetting> UserTwoFactorSettings => Set<UserTwoFactorSetting>();

        public DbSet<UserRecoveryCode> UserRecoveryCodes => Set<UserRecoveryCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserTwoFactorSetting>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<UserTwoFactorSetting>()
                .HasMany(s => s.RecoveryCodes)
                .WithOne(rc => rc.UserTwoFactorSetting)
                .HasForeignKey(rc => rc.UserTwoFactorSettingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
