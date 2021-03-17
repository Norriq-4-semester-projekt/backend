using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<ActionResult> AddAsync(T entity);

        //Task<ActionResult> GetByQueryAsync(T entity);
        Task<IEnumerable<T>> GetAll();

        Task<ActionResult> UpdateByQueryAsync(T entity, T u1);

        Task<ActionResult> DeleteByQueryAsync(T entity);
    }
}