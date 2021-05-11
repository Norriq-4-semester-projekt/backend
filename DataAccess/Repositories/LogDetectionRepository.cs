using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class LogDetectionRepository : ILogDetectionRepository
    {
        public Task<ActionResult> AddAsync(Data entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(Data entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Data>> GetAll()
        {
            throw new NotImplementedException();
        }

        public bool LogDetectionData(Data data)
        {
            var indexResponse = ElasticConnection.Instance.client.Index<Data>(data, i => i.Index("mldetection"));
            return indexResponse.IsValid;
        }

        public Task<ActionResult> UpdateByQueryAsync(Data entity, Data u1)
        {
            throw new NotImplementedException();
        }
    }
}