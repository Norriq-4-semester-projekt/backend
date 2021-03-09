using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataAccess.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<T> GetByQueryAsync(T entity);
        Task<IEnumerable<T>> GetAll();
        Task<T> UpdateAsync(T entity);
        Task<int> DeleteByQueryAsync(T entity);

    }
}
