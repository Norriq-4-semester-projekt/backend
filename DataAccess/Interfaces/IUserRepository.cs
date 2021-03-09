using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DataAccess.Repositories;
using System.Collections.Generic;

namespace DataAccess.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        

    }
}
