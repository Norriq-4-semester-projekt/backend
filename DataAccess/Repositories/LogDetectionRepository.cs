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

        public async Task<IEnumerable<Data>> GetAll()
        {
            var response = await ElasticConnection.Instance.client.SearchAsync<Data>(s => s
               .Index("mldetection")
               .Size(5000)
               .Sort(ss => ss
               .Descending(de => de.Timestamp))

                   .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .MatchAll()
                                )
                            //.Filter(f => f
                            //    .DateRange(dr => dr
                            //        .Field("@timestamp")
                            //        .GreaterThanOrEquals("now-1d")
                            //        //.GreaterThanOrEquals(entity.DateRange)

                            //        )
                            //    )
                            )
                        )
                   );

            return response.Documents.AsEnumerable();
        }

        public bool LogDetectionData(Data data)
        {
            var indexResponse = ElasticConnection.Instance.client.Index<Data>(data, i => i.Index("detectml"));
            return indexResponse.IsValid;
        }

        public Task<ActionResult> UpdateByQueryAsync(Data entity, Data u1)
        {
            throw new NotImplementedException();
        }
    }
}