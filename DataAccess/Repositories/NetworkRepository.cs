using DataAccess.Entities;
using DataAccess.Entities.Network;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class NetworkRepository : INetworkRepository
    {
        public Task<ActionResult> AddAsync(NetworkData entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(NetworkData entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NetworkData>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<NetworkData>> GetAllBytesIn()
        {
            var response = ElasticConnection.Instance.client.Search<NetworkData>(s => s
                .Index("metricbeat-*")
                .Size(10000)
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

            return response.Documents.AsEnumerable<NetworkData>();
        }

        public async Task<IEnumerable<NetworkData>> GetAllBytesOut()
        {
            var response = ElasticConnection.Instance.client.Search<NetworkData>(s => s
                .Index("metricbeat-*")
                .Size(5000)
                .Sort(ss => ss
                .Descending(de => de.Timestamp))

                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("host.network.out.bytes")
                                    )
                                )
                            )
                        )
                .DocValueFields(dvf => dvf
                    .Fields("host.network.out.bytes", "@timestamp"))
                 );

            return response.Documents.AsEnumerable<NetworkData>();
        }

        public async Task<Data> GetLatestBytesIn()
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
                        .DateHistogram("NetworkBytesInDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("AvgBytesIn", avg => avg
                            .Field("host.network.in.bytes"))
                        )
                    )
                ));
            Data networksData = new Data();
            networksData.Timestamp = response.Aggregations.DateHistogram("NetworkBytesInDateHistogram").Buckets.FirstOrDefault().KeyAsString;
            networksData.Value = (float)response.Aggregations.DateHistogram("NetworkBytesInDateHistogram").Buckets.FirstOrDefault().AverageBucket("AvgBytesIn").Value.Value;
            return networksData;

            return null;
        }

        public async Task<Data> GetLatestBytesOut()
        {
            var response = await ElasticConnection.Instance.client.SearchAsync<Data>(s => s
                .Index("metricbeat-*")
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("host.network.out.bytes")
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
                        .DateHistogram("NetworkBytesOutDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("AvgBytesOut", avg => avg
                            .Field("host.network.out.bytes"))
                        )
                    )
                ));
            Data networksData = new Data();
            networksData.Timestamp = response.Aggregations.DateHistogram("NetworkBytesOutDateHistogram").Buckets.FirstOrDefault().KeyAsString;
            networksData.Value = (float)response.Aggregations.DateHistogram("NetworkBytesOutDateHistogram").Buckets.FirstOrDefault().AverageBucket("AvgBytesOut").Value.Value;
            return networksData;

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

            return networksDataList;
            return null;
        }

        public Task<ActionResult> UpdateByQueryAsync(NetworkData entity, NetworkData u1)
        {
            throw new NotImplementedException();
        }
    }
}