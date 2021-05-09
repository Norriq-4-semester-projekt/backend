﻿using DataAccess.Entities;
using DataAccess.Entities.Cpu;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class CpuRepository : ICpuRepository
    {
        //public IActionResult GetDataCpu()
        //{
        //    try
        //    {
        //        var rs = ElasticConnection.Instance.client.Search<dynamic>(s => s
        //        .Index("metricbeat-*")
        //            .Query(q => q
        //                .Bool(b => b
        //                    .Should(sh => sh
        //                        .MatchPhrase(mp => mp
        //                            .Field("host.name").Query("vmi316085.contaboserver.net")
        //                            .Field("event.dataset").Query("system.cpu")
        //                        )
        //                    )
        //                    .Filter(f => f
        //                        .DateRange(dr => dr
        //                            .Field("@timestamp")
        //                            .GreaterThanOrEquals("now-5m")
        //                            )
        //                        )
        //                    )
        //                )
        //            .Aggregations(aggs1 => aggs1
        //                .DateHistogram("myCpuDateHistogram", date => date
        //                .Field("@timestamp")
        //                .CalendarInterval(DateInterval.Minute)
        //                .Aggregations(aggs => aggs
        //                    .Average("AvgUserCpu", auc => auc
        //                    .Field("system.cpu.user.pct")
        //                    )
        //                .Average("AvgSystemCpu", asc => asc
        //                .Field("system.cpu.system.pct")
        //                )
        //                .Max("CpuCoresMax", mcpu => mcpu
        //                .Field("system.cpu.cores")
        //            ).BucketScript("CpuCalc", bs => bs
        //                .BucketsPath(bp => bp
        //                .Add("user", "AvgUserCpu")
        //                .Add("system", "AvgSystemCpu")
        //                .Add("cores", "CpuCoresMax"))
        //                .Script("(params.user + params.system) / params.cores")))
        //        )
        //                )
        //    );
        //        if (rs.Aggregations.Count > 0)
        //        {
        //            Dictionary<String, Object> etellerandet = new Dictionary<String, Object>();
        //            var dateHistogram = rs.Aggregations.DateHistogram("myCpuDateHistogram");
        //            List<Object> list = new List<Object>();

        //            foreach (DateHistogramBucket item in dateHistogram.Buckets)
        //            {
        //                Dictionary<string, string> newlist = new Dictionary<string, string>();
        //                Dictionary<string, string> cpuPair = new Dictionary<string, string>();

        //                foreach (var test89 in item.Keys)
        //                {
        //                    item.TryGetValue(test89, out IAggregate a);
        //                    ValueAggregate valueAggregate = a as ValueAggregate;

        //                    cpuPair.Add(test89, valueAggregate.Value.ToString());
        //                    Console.WriteLine(test89 + ": " + (valueAggregate.Value));
        //                    Console.WriteLine(test89 + ": " + valueAggregate.ValueAsString);
        //                }
        //                list.Add(cpuPair);
        //                //StatsAggregate test = (StatsAggregate)item.Values.FirstOrDefault();
        //                //newlist.Add("Timestamp", item.Date.ToString());
        //                //newlist.Add("min", test.Min.ToString());
        //                //newlist.Add("max", test.Max.ToString());
        //                //newlist.Add("avg", test.Average.ToString());
        //                //list.Add(newlist);
        //            }

        //            return Ok(JsonSerializer.Serialize(list));
        //        }
        //        return new StatusCodeResult(200);
        //    }
        //    catch (Exception)
        //    {
        //        return new StatusCodeResult(500);
        //    }
        //}
        public Task<ActionResult> AddAsync(CpuData entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(CpuData entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CpuData>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<Data> GetLatest()
        {
            var response = await ElasticConnection.Instance.client.SearchAsync<Data>(s => s
                .Index("metricbeat-*")
                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("system.cpu.user.pct")
                                    .Field("system.cpu.system.pct")
                                    .Field("system.cpu.cores")
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
                    .Aggregations(aggs1 => aggs1
                        .DateHistogram("myCpuDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("AvgUserCpu", auc => auc
                            .Field("system.cpu.user.pct")
                            )
                        .Average("AvgSystemCpu", asc => asc
                        .Field("system.cpu.system.pct")
                        )
                        .Max("CpuCoresMax", mcpu => mcpu
                        .Field("system.cpu.cores")
                    ).BucketScript("CpuCalc", bs => bs
                        .BucketsPath(bp => bp
                        .Add("user", "AvgUserCpu")
                        .Add("system", "AvgSystemCpu")
                        .Add("cores", "CpuCoresMax"))
                        .Script("(params.user + params.system) / params.cores")))
                )
                        )
            );
            Data cpuData = new Data();
            cpuData.Timestamp = response.Aggregations.DateHistogram("myCpuDateHistogram").Buckets.FirstOrDefault().KeyAsString;
            cpuData.Value = (float)response.Aggregations.DateHistogram("myCpuDateHistogram").Buckets.FirstOrDefault().AverageBucket("CpuCalc").Value.Value;
            return cpuData;

            Console.WriteLine(response.DebugInformation);

            return null;
        }

        public Task<List<CpuData>> GetLatestMonth()
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> UpdateByQueryAsync(CpuData entity, CpuData u1)
        {
            throw new NotImplementedException();
        }
    }
}