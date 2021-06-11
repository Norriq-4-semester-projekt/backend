﻿using DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ILogDetectionRepository : IGenericRepository<Data>
    {
        bool LogDetectionData(Data data);

        bool LogPredictionData(Data data);

        bool LogChangepointData(Data data);
        bool LogPredictionSystemLoad(Data data);


        Task<IEnumerable<Data>> GetAllPredictions();

        Task<IEnumerable<Data>> GetAllChangepoints();
    }
}