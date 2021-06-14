using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class LogDetectionRepository : ILogDetectionRepository
    {
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
            var response = await ElasticConnection.Instance.Client.SearchAsync<Data>(s => s
               .Index("spike-detect")
               .Size(10000)
               .Sort(ss => ss
               .Descending(de => de.Timestamp))

                   .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .MatchAll()
                                )
                            //.Filter(f => f
                            //    .DateRange(dr => dr
                            //        .Field("@timestamp")
                            //        .GreaterThanOrEquals("now-1d")
                            //        //.GreaterThanOrEquals(entity.DateRange)

                            //        )
                            //    )
                            )
                        )
                   );

            return response.Documents.AsEnumerable();
        }

        public async Task<IEnumerable<Data>> GetAllPredictions()
        {
            var response = await ElasticConnection.Instance.Client.SearchAsync<Data>(s => s
               .Index("value-predict")
               .Size(10000)
               .Sort(ss => ss
               .Descending(de => de.Timestamp))

                   .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .MatchAll()
                                )
                            //.Filter(f => f
                            //    .DateRange(dr => dr
                            //        .Field("@timestamp")
                            //        .GreaterThanOrEquals("now-1d")
                            //        //.GreaterThanOrEquals(entity.DateRange)

                            //        )
                            //    )
                            )
                        )
                   );

            return response.Documents.AsEnumerable();
        }

        public async Task<IEnumerable<Data>> GetAllSystemLoadPredictions()
        {
            var response = await ElasticConnection.Instance.Client.SearchAsync<Data>(s => s
               .Index("forudsig")
               .Size(10000)
               .Sort(ss => ss
               .Descending(de => de.Timestamp))

                   .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .MatchAll()
                                )
                            //.Filter(f => f
                            //    .DateRange(dr => dr
                            //        .Field("@timestamp")
                            //        .GreaterThanOrEquals("now-1d")
                            //        //.GreaterThanOrEquals(entity.DateRange)

                            //        )
                            //    )
                            )
                        )
                   );

            return response.Documents.AsEnumerable();
        }
        public async Task<IEnumerable<Data>> GetAllCpupctTimePredictions()
        {
            var response = await ElasticConnection.Instance.Client.SearchAsync<Data>(s => s
               .Index("predict-cpupct-time")
               .Size(10000)

                   .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .MatchAll()
                                )
                            )
                        )
                   );

            return response.Documents.AsEnumerable();
        }

        public async Task<IEnumerable<Data>> GetAllChangepoints()
        {
            var response = await ElasticConnection.Instance.Client.SearchAsync<Data>(s => s
               .Index("changepointml5")
               .Size(10000)
               .Sort(ss => ss
               .Descending(de => de.Timestamp))

                   .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .MatchAll()
                                )
                            //.Filter(f => f
                            //    .DateRange(dr => dr
                            //        .Field("@timestamp")
                            //        .GreaterThanOrEquals("now-1d")
                            //        //.GreaterThanOrEquals(entity.DateRange)

                            //        )
                            //    )
                            )
                        )
                   );

            return response.Documents.AsEnumerable();
        }

        public bool LogDetectionData(Data data)
        {
            var indexResponse = ElasticConnection.Instance.Client.Index<Data>(data, i => i.Index("spike-detect"));
            return indexResponse.IsValid;
        }

        public bool LogPredictionData(Data data)
        {
            var indexResponse = ElasticConnection.Instance.Client.Index<Data>(data, i => i.Index("value-predict"));
            return indexResponse.IsValid;
        }

        public bool LogPredictionDataCpupctTime(Data data)
        {
            var indexResponse = ElasticConnection.Instance.Client.Index<Data>(data, i => i.Index("predict-cpupct-time"));
            return indexResponse.IsValid;
        }

        public bool LogChangepointData(Data data)
        {
            var indexResponse = ElasticConnection.Instance.Client.Index<Data>(data, i => i.Index("changepointml5"));
            return indexResponse.IsValid;
        }

        public bool LogPredictionSystemLoad(Data data)
        {
            var indexResponse = ElasticConnection.Instance.Client.Index<Data>(data, i => i.Index("forudsig"));
            return indexResponse.IsValid;
        }

        public Task<ActionResult> UpdateByQueryAsync(Data entity, Data u1)
        {
            throw new NotImplementedException();
        }
    }
}