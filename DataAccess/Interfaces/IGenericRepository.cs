using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);

        Task<T> GetByQueryAsync(T entity);

        Task<IEnumerable<T>> GetAll();

        Task<int> UpdateByQueryAsync(T entity, T u1);

        Task<int> DeleteByQueryAsync(T entity);
    }
}