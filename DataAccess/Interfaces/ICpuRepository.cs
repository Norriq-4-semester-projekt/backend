using DataAccess.Entities;
using DataAccess.Entities.Cpu;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ICpuRepository : IGenericRepository<CpuData>
    {
        Task<Data> GetLatest();
    }
}