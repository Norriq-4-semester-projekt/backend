using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class DataRepository : IDataRepository
    {
        private ElasticClient client;

        //private ConnectionSettings settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("users"); // localhost
        private ConnectionSettings settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users"); // vps

        public DataRepository()
        {
            settings.BasicAuthentication("elastic", "changeme"); // ElasticSearch Username and Password
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
            client = new ElasticClient(settings);
        }

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
            try
            {
                List<Data> data = new List<Data>();

                var response = await ElasticConnection.Instance.client.SearchAsync<Data>(s => s
                .Index("metricbeat-*")
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Should(sh => sh
                            .MatchPhrase(mp => mp
                                .Field("hostname").Query("vmi316085.contaboserver.net")
                                .Field("event.dataset").Query("system.network")
                            )
                        )
                        .Filter(f => f
                            .DateRange(dr => dr
                                .Field("@timestamp")
                                .GreaterThanOrEquals("now-2h")
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
                            .Average("AVGnetOut", avg => avg
                            .Field("host.network.out.bytes"))
                            .Max("MAXnetIN", max => max
                            .Field("host.network.in.bytes"))
                            .Max("MAXnetOUT", max => max
                            .Field("host.network.out.bytes"))
                            )
                        )
                    )
                );

                
                if (response.Aggregations.Count > 0)
                {
                    Console.WriteLine(response.DebugInformation);
                    var dateHistogram = response.Aggregations.DateHistogram("myNetworkDateHistogram");
                    foreach (DateHistogramBucket item in dateHistogram.Buckets)
                    {
                        //Data d = new Data();
                        //d.Timestamp = item.KeyAsString;
                        Dictionary<string, dynamic> newlist = new Dictionary<string, dynamic>();

                        foreach ( var item2 in item.Keys)
                        {
                            item.TryGetValue(item2, out IAggregate a);
                            ValueAggregate vg = a as ValueAggregate;
                            
                        }
                        //d.AVGnetIn = item.Keys;
                    }
                    
                    /*
                    
                    var dateHistogram = response.Aggregations.DateHistogram("myNetworkDateHistogram");
                    List<Object> list = new List<Object>();
                    foreach (DateHistogramBucket item in dateHistogram.Buckets)
                    {
                        Dictionary<string, dynamic> newlist = new Dictionary<string, dynamic>();
                        newlist.Add("Timestamp", item.KeyAsString);

                        foreach (var item2 in item.Keys)
                        {
                            item.TryGetValue(item2, out IAggregate a);
                            ValueAggregate valueAggregate = a as ValueAggregate;
                            newlist.Add(item2, valueAggregate.Value);
                        }
                        list.Add(newlist);
                    }
                    return Ok(JsonSerializer.Serialize(list));
                    */
                }
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<ActionResult> UpdateByQueryAsync(Data entity, Data u1)
        {
            throw new NotImplementedException();
        }
    }
}
