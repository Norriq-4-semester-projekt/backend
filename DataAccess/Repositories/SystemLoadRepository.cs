using DataAccess.Entities;
using DataAccess.Entities.Load;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class SystemLoadRepository : ISystemLoadRepository
    {
        public Task<ActionResult> AddAsync(SystemLoadData entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(SystemLoadData entity)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SystemLoadData>> GetAll()
        {
            var response = await ElasticConnection.Instance.Client.SearchAsync<SystemLoadData>(s => s
                .Index("metricbeat-*")
                .Size(20000)
                .Sort(ss => ss
                .Descending(de => de.Timestamp))

                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("system.load.15")
                                    )
                                )
                            )
                        )
                .DocValueFields(dvf => dvf
                    .Fields("system.load.15", "@timestamp"))
                 );

            return response.Documents.AsEnumerable<SystemLoadData>();
        }

        public async Task<Data> GetLatest()
        {
            var response = await ElasticConnection.Instance.Client.SearchAsync<Data>(s => s
                .Index("metricbeat-*")
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("system.load.15")
                                    )
                                )
                            .Filter(f => f
                                .DateRange(dr => dr
                                    .Field("@timestamp")
                                    .GreaterThanOrEquals("now-1m")
                                    )
                                )
                            )
                            )
                    .Aggregations(aggs => aggs
                        .DateHistogram("SystemLoadDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("AvgSystemLoad", avg => avg
                            .Field("system.load.15"))
                        )
                    )
                ));
            Data systemloadData = new()
            {
                Timestamp = response.Aggregations.DateHistogram("SystemLoadDateHistogram").Buckets.FirstOrDefault()
                    .KeyAsString,
                Value = (float)response.Aggregations.DateHistogram("SystemLoadDateHistogram").Buckets.FirstOrDefault()
                    .AverageBucket("AvgSystemLoad").Value.Value
            };
            return systemloadData;
        }

        public Task<ActionResult> UpdateByQueryAsync(SystemLoadData entity, SystemLoadData u1)
        {
            throw new NotImplementedException();
        }
    }
}