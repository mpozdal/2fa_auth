using System.Linq.Expressions;

namespace TwoFactorService.Application.Interfaces
{
    public interface IGenericRepository<T> where T: class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
