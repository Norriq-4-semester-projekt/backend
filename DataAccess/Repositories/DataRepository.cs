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
                List<Data> data = new List<Data>();

                var response = await ElasticConnection.Instance.client.SearchAsync<dynamic>(s => s
                .Index("metricbeat-*")
                    .Query(q => q
                        .Exists(c => c
                            .Field("host.network.*")
                        )
                    )
                    .Source(src => src
                        .Includes(i => i
                            .Field("host.network.*")
                        )
                    )
                );

                if (response.Hits.Count > 0)
                {
                    Console.WriteLine(response.DebugInformation);
                    foreach (var hit in response.Hits)

                    {
                        Data d = hit.Source;
                        data.Add(d);
                    }
                }
                return data;
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