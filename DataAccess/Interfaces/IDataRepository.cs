using DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDataRepository : IGenericRepository<NetworksData>
    {
        Task<Data> GetLatest();
        Task<List<Data>> GetLatestMonth();

    }
}