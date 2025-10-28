namespace AuthService.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
