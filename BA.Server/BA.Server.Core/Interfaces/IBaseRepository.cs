using System.Linq.Expressions;

namespace BA.Server.Core.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);  // 為了支援 string ID
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task DeleteAsync(string id);  // 為了支援 string ID
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(string id);  // 為了支援 string ID
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}