﻿using DataAccess.Entities;
using DataAccess.Entities.Memory;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class MemoryRepository : IMemoryRepository
    {
        public Task<ActionResult> AddAsync(MemoryData entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(MemoryData entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MemoryData>> GetAll()
        {
            var response = ElasticConnection.Instance.client.Search<MemoryData>(s => s
                .Index("metricbeat-*")
                .Size(1000)
                .Sort(ss => ss
                .Descending(de => de.Timestamp))

                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("system.memory.actual.used.bytes")
                                    )
                                )
                            )
                        )
                .DocValueFields(dvf => dvf
                    .Fields("system.memory.actual.used.bytes", "@timestamp"))
                 );

            Console.WriteLine(response.DebugInformation);
            return (Task<IEnumerable<MemoryData>>)response.Documents.AsEnumerable<MemoryData>();
        }

        public async Task<Data> GetLatest()
        {
            var response = await ElasticConnection.Instance.client.SearchAsync<Data>(s => s
                .Index("metricbeat-*")
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("system.memory.actual.used.bytes")
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
                        .DateHistogram("MemoryDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("MemoryUsedByte", avg => avg
                            .Field("system.memory.actual.used.bytes"))
                        )
                    )
                ));
            Data memoryData = new Data();
            memoryData.Timestamp = response.Aggregations.DateHistogram("MemoryDateHistogram").Buckets.FirstOrDefault().KeyAsString;
            memoryData.Value = (float)response.Aggregations.DateHistogram("MemoryDateHistogram").Buckets.FirstOrDefault().AverageBucket("MemoryUsedByte").Value.Value;
            return memoryData;

            Console.WriteLine(response.DebugInformation);

            return null;
        }

        public Task<ActionResult> UpdateByQueryAsync(MemoryData entity, MemoryData u1)
        {
            throw new NotImplementedException();
        }
    }
}