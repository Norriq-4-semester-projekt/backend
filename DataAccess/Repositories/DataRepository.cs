using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
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
                var response = await ElasticConnection.Instance.client.SearchAsync<dynamic>(s => s
                .Index("metricbeat-*")
                    .Size(0)
               .Query(q => q
                   .Bool(b => b
                       .Should(sh => sh
                           .MatchPhrase(mp => mp
                               .Field("hostname").Query("vmi316085.contaboserver.net")
                           )
                       )
                       .Filter(f => f
                           .DateRange(dr => dr
                               .Field("@timestamp")
                               .GreaterThanOrEquals("now-15m")
                               )
                           )
                       )
                   )
               .Aggregations(aggs => aggs
                   .DateHistogram("myNetworkDateHistogram", date => date
                   .Field("@timestamp")
                   .CalendarInterval(DateInterval.Minute)
                   .Aggregations(aggs => aggs
                       .Average("BytesIn", avg => avg
                       .Field("host.network.in.bytes"))
                       )
                   )
                   )
                );
                Data list = new Data();
                list.BytesList = new List<Data>();
                if (response.Aggregations.Count > 0)
                {
                    foreach (DateHistogramBucket item in response.Aggregations.DateHistogram("myNetworkDateHistogram").Buckets)
                    {
                        Data d = new Data();
                        Dictionary<string, double?> netwirk = new Dictionary<string, double?>();
                        foreach (var test in item.Keys)
                        {
                            item.TryGetValue(test, out IAggregate a);
                            ValueAggregate valueAggregate = a as ValueAggregate;
                            d.BytesIn = (long)valueAggregate.Value;
                            d.Timestamp = item.KeyAsString;
                            netwirk.Add(test, valueAggregate.Value);
                            Console.WriteLine(test + ": " + d.BytesIn + " " + "Timestamp: " + d.Timestamp);
                        }

                        list.BytesList.Add(d);
                    }
                }
                return (IEnumerable<Data>)list;
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