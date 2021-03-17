using DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);

        Task<int> GetByQueryAsync(T entity);

        Task<int> GetAll(List<User> users);

        Task<int> UpdateByQueryAsync(T entity, T u1);

        Task<int> DeleteByQueryAsync(T entity);
    }
}