using DataAccess.Entities;
using DataAccess.Entities.Load;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ISystemLoadRepository : IGenericRepository<SystemLoadData>
    {
        Task<Data> GetLatestLoad15Data();
    }
}
