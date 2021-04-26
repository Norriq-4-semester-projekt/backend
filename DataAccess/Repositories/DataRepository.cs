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

            //try
            //{
            //    var response = await ElasticConnection.Instance.client.SearchAsync<dynamic>(s => s
            //    .Index("metricbeat-*")
            //        .Size(0)
            //   .Query(q => q
            //       .Bool(b => b
            //           .Should(sh => sh
            //               .MatchPhrase(mp => mp
            //                   .Field("hostname").Query("vmi316085.contaboserver.net")
            //               )
            //           )
            //           .Filter(f => f
            //               .DateRange(dr => dr
            //                   .Field("@timestamp")
            //                   .GreaterThanOrEquals("now-30s")
            //                   )
            //               )
            //           )
            //       )
            //   .Aggregations(aggs => aggs
            //       .DateHistogram("myNetworkDateHistogram", date => date
            //       .Field("@timestamp")
            //       .CalendarInterval(DateInterval.Minute)
            //       .Aggregations(aggs => aggs
            //           .Average("BytesIn", avg => avg
            //           .Field("host.network.in.bytes"))
            //           )
            //       )
            //       )
            //    );
            //    Data list = new Data();
            //    list.BytesList = new List<Data>();
            //    if (response.Aggregations.Count > 0)
            //    {
            //        response.Aggregations.DateHistogram("myNetworkDateHistogram").Buckets.GetEnumerator().MoveNext();
            //        DateHistogramBucket keys = response.Aggregations.DateHistogram("myNetworkDateHistogram").Buckets.GetEnumerator().Current;
            //        foreach (DateHistogramBucket item in response.Aggregations.DateHistogram("myNetworkDateHistogram").Buckets)
            //        {
            //            Data d = new Data();
            //            Dictionary<string, double?> netwirk = new Dictionary<string, double?>();
            //            foreach (var test in item.Keys)
            //            {
            //                item.TryGetValue(test, out IAggregate a);
            //                ValueAggregate valueAggregate = a as ValueAggregate;
            //                d.BytesIn = (long)valueAggregate.Value;
            //                d.Timestamp = item.KeyAsString;
            //                netwirk.Add(test, valueAggregate.Value);
            //                Console.WriteLine(test + ": " + d.BytesIn + " " + "Timestamp: " + d.Timestamp);
            //            }

            //            list.BytesList.Add(d);
            //        }
            //    }
            //    return (IEnumerable<Data>)list;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
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
            //networksData.Host.Network.In.Bytes = (ValueAggregate)buckets.Values.FirstOrDefault().

            //var response = ElasticConnection.Instance.client.Search<NetworksData>(s => s
            //    .Index("metricbeat-*")
            //    .Size(1)
            //    .Sort(ss => ss
            //    .Descending(de => de.Timestamp))

            //        .Query(q => q
            //            .Bool(b => b
            //                .Must(sh => sh
            //                    .Exists(ex => ex
            //                        .Field("host.network.in.bytes")
            //                        )
            //                    )
            //                )
            //            )
            //    .DocValueFields(dvf => dvf
            //        .Fields("host.network.in.bytes", "@timestamp"))
            //     );
            Console.WriteLine(response.DebugInformation);

            //return response.Documents.AsEnumerable<NetworksData>().FirstOrDefault();
            return null;
            //var buffer = System.Text.Encoding.UTF8.GetBytes(dataAsJson);
            //var byteContent = new ByteArrayContent(buffer);
            //byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //// Create a New HttpClient object.
            //HttpClient client = new HttpClient();

            //// Call asynchronous network methods in a try/catch block to handle exceptions
            //bool test = false;
            //try
            //{
            //    var httpResponse = await client.PostAsync("https://localhost:44368/ML", byteContent);
            //    string responseBody = await httpResponse.Content.ReadAsStringAsync();
            //    test = JsonConvert.DeserializeObject<bool>(responseBody);
            //    httpResponse.EnsureSuccessStatusCode();
            //    // Above three lines can be replaced with new helper method below
            //    // string responseBody = await client.GetStringAsync(uri);

            //    Console.WriteLine(responseBody);
            //}
            //catch (HttpRequestException e)
            //{
            //    Console.WriteLine("\nException Caught!");
            //    Console.WriteLine("Message :{0} ", e.Message);
            //}
            //// Need to call dispose on the HttpClient object
            //// when done using it, so the app doesn't leak resources
            //client.Dispose();

            //if (test)
            //{
            //    var telegramBot = new TelegramBotClient("1618808038:AAHs2nHXf_sYeOIgwiIr1nxqMz6Uul-w4nA");
            //    await telegramBot.SendTextMessageAsync("-1001399759228", "Der kommer Spyyd!");
            //}

            //return test;
        }

        public Task<ActionResult> UpdateByQueryAsync(NetworksData entity, NetworksData u1)
        {
            throw new NotImplementedException();
        }
    }
}