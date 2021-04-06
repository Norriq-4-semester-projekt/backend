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
                        .Bool(b => b
                            .Should(sh => sh
                                .Match(c => c
                                    .Field("host.network.in.bytes")
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
                    .Source(src => src
                        .Includes(i => i
                            .Field("host.network.in.bytes")
                        )
                    )
                );

                if (response.Hits.Count > 0)
                {
                    //Console.WriteLine(response.DebugInformation);
                    var dataResponse = response.Hits;

                    foreach (Hit<dynamic> item in dataResponse)
                    {
                        String timestamp = null;
                        object bytesIn = 0;
                        Dictionary<string, dynamic> test = item.Source;
                        test.TryGetValue("host", out var host);
                        Dictionary<string, dynamic> test2 = host;
                        test2.TryGetValue("network", out var network);
                        Dictionary<string, dynamic> test3 = network;
                        test3.TryGetValue("in", out var input);
                        Dictionary<string, dynamic> test4 = input;
                        test4.TryGetValue("bytes", out var final);
                        long test144 = final;
                        Data d = new Data
                        {
                            Bytes = test144
                        };
                        Console.WriteLine(d.Bytes);
                        data.Add(d);
                        Console.WriteLine(data);
                    }
                }

                return data;
            }
            catch (Exception e)
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