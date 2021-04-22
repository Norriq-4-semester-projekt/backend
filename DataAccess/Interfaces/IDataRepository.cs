using DataAccess.Entities;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDataRepository : IGenericRepository<NetworksData>
    {
        Task<NetworksData> GetLatest();
    }
}