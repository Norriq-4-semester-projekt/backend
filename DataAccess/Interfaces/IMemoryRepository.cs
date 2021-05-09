using DataAccess.Entities;
using DataAccess.Entities.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IMemoryRepository : IGenericRepository<MemoryData>
    {
        Task<Data> GetLatest();

    }
}
