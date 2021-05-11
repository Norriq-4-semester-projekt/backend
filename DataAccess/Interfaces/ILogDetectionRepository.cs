using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ILogDetectionRepository : IGenericRepository<Data>
    {
        bool LogDetectionData(Data data);
    }
}