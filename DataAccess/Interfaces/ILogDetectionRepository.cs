using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ILogDetectionRepository : IGenericRepository<Data>
    {
        bool LogDetectionData(Data data);

        bool LogPredictionData(Data data);

        Task<IEnumerable<Data>> GetAllPredictions();
    }
}