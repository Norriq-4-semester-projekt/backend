using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DataAccess.Repositories;
using System.Collections.Generic;

namespace DataAccess.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<ActionResult> AddAsync(User entity);
        ActionResult<User> DeleteByQueryAsync(User entity);
        Task<IEnumerable<User>> GetAll();
        Task<User> GetByQueryAsync(User entity);
        Task<User> UpdateAsync(User entity);
        //Task<User> IGenericRepository<User>.AddAsync(User entity);
        //Task<User> IGenericRepository<User>.DeleteByQueryAsync(User entity)
        



    }
}
