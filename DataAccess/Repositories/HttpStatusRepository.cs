using DataAccess.Service;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class HttpStatusRepository : IHttpStatusRepository
    {
        private ElasticClient client;

        //private ConnectionSettings settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("users"); // localhost
        private ConnectionSettings settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users"); // vps

        public HttpStatusRepository()
        {
            settings.BasicAuthentication("elastic", "changeme"); // ElasticSearch Username and Password
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
            client = new ElasticClient(settings);
        }

        public Task<ActionResult> AddAsync(Entities.HttpStatus entity)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteByQueryAsync(Entities.HttpStatus entity)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Entities.HttpStatus>> GetAll()
        {
            var response = ElasticConnection.Instance.client.Search<Entities.HttpStatus>(s => s
                .Index("packetbeat-*")
                .Size(1000)
                .Sort(ss => ss
                .Descending(de => de.Timestamp))

                    .Query(q => q
                        .Bool(b => b
                            .Must(sh => sh
                                .Exists(ex => ex
                                    .Field("status")
                                    )
                                )
                            )
                        )
                .DocValueFields(dvf => dvf
                    .Fields("status", "@timestamp"))
                 );

            return response.Documents.AsEnumerable<Entities.HttpStatus>();
        }

        public Task<ActionResult> UpdateByQueryAsync(Entities.HttpStatus entity, Entities.HttpStatus u1)
        {
            throw new NotImplementedException();
        }
    }
}