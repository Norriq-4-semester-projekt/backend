using DataAccess.Entities;
using DataAccess.Entities.Load;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ISystemLoadRepository : IGenericRepository<SystemLoadData>
    {
        Task<Data> GetLatest();
    }
}