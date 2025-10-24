namespace TwoFactorService.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITwoFactorSettingsRepository Settings { get; }
        IRecoveryCodeRepository RecoveryCodes { get; }

        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
