using DataAccess.Entities;
using DataAccess.Entities.Network;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface INetworkRepository : IGenericRepository<NetworkData>
    {
        Task<Data> GetLatestBytesIn();
        Task<Data> GetLatestBytesOut();
        Task<IEnumerable<NetworkData>> GetAllBytesIn();
        Task<IEnumerable<NetworkData>> GetAllBytesOut();


        Task<List<Data>> GetLatestMonth();

    }
}