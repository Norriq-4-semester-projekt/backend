using DataAccess.Entities;
using DataAccess.Entities.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ICpuRepository : IGenericRepository<CpuData>
    {
        Task<Data> GetLatest();
    }
}
