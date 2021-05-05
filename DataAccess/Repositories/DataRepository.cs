using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DataAccess.Repositories
{
    public class DataRepository : IDataRepository
    {
        public Task<ActionResult> AddAsync(NetworksData entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(NetworksData entity)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<NetworksData>> GetAll()
        {
            var response = ElasticConnection.Instance.client.Search<NetworksData>(s => s
                .Index("metricbeat-*")
                .Size(1000)
                .Sort(ss => ss
                .Descending(de => de.Timestamp))

                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("host.network.in.bytes")
                                    )
                                )
                            )
                        )
                .DocValueFields(dvf => dvf
                    .Fields("host.network.in.bytes", "@timestamp"))
                 );

            Console.WriteLine(response.DebugInformation);
            return response.Documents.AsEnumerable<NetworksData>();
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
                                    .Field("host.network.in.bytes")
                                    )
                                ).Filter(f => f
                                .DateRange(dr => dr
                                    .Field("@timestamp")
                                    .GreaterThanOrEquals("now-1m")
                                    )
                                )
                            )
                            )
                    .Aggregations(aggs => aggs
                        .DateHistogram("myNetworkDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("AVGnetIN", avg => avg
                            .Field("host.network.in.bytes"))
                        )
                    )
                ));
            Data networksData = new Data();
            networksData.Timestamp = response.Aggregations.DateHistogram("myNetworkDateHistogram").Buckets.FirstOrDefault().KeyAsString;
            networksData.Value = (float)response.Aggregations.DateHistogram("myNetworkDateHistogram").Buckets.FirstOrDefault().AverageBucket("AVGnetIN").Value.Value;
            return networksData;

            Console.WriteLine(response.DebugInformation);

            return null;
        }
        public async Task<List<Data>> GetLatestMonth()
        {
            var response = await ElasticConnection.Instance.client.SearchAsync<Data>(s => s
                .Index("metricbeat-*")
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("host.network.in.bytes")
                                    )
                                ).Filter(f => f
                                .DateRange(dr => dr
                                    .Field("@timestamp")
                                    .GreaterThanOrEquals("now-1M")
                                    )
                                )
                            )
                            )
                    .Aggregations(aggs => aggs
                        .DateHistogram("myNetworkDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("AVGnetIN", avg => avg
                            .Field("host.network.in.bytes"))
                        )
                    )
                ));

            List<Data> networksDataList = new List<Data>();
            var dateHistogram = response.Aggregations.DateHistogram("myNetworkDateHistogram");
            foreach (var item in dateHistogram.Buckets)
            {
            Data networksData = new Data();
            networksData.Timestamp = item.KeyAsString;
            networksData.Value = (float)item.AverageBucket("AVGnetIN").Value.Value;
            networksDataList.Add(networksData);

            }
            
            Console.WriteLine(response.DebugInformation);
            return networksDataList;
            return null;


        }

        public Task<ActionResult> UpdateByQueryAsync(NetworksData entity, NetworksData u1)
        {
            throw new NotImplementedException();
        }
    }
}