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
        Task<Data> GetLatestPacketsIn();

        Task<Data> GetLatestPacketsOut();



        Task<IEnumerable<NetworkData>> GetLikeAllData();
        Task<IEnumerable<NetworkData>> GetAllBytesIn();

        Task<IEnumerable<NetworkData>> GetAllBytesOut();
        Task<IEnumerable<NetworkData>> GetAllPacketsIn();
        Task<IEnumerable<NetworkData>> GetAllPacketsOut();





    }
}