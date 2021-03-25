﻿using DataAccess;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Controllers.v1_0
{
    // [ApiVersion("0.9", Deprecated = true)] // Set previous version as deprecated
    [ApiVersion("1.0")] // Set version of controller
    [ApiController]
    [Route("v{version:apiVersion}/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public WeatherForecastController(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //private readonly ILogger<WeatherForecastController> _logger;

        //public WeatherForecastController(ILogger<WeatherForecastController> logger)
        //{
        //    _logger = logger;
        //}

        [Authorize]
        [HttpGet]
        public IActionResult ProduceError()
        {
            try
            {
                // https://youtu.be/0acSdHJfk64?list=PLUOequmGnXxOFPJv8H7DNIappcta9brtN

                //Proof of concept

                //_logger.LogInformation(message: "First Log");
                var rng = new Random();

                if ((rng.Next(0, 5) < 2))
                {
                    throw new Exception(message: "Random Bad Call");
                }
                return Ok(
                     Enumerable.Range(1, 5).Select(index => new WeatherForecast
                     {
                         Date = DateTime.Now.AddDays(index),
                         TemperatureC = rng.Next(-20, 55),
                         Summary = Summaries[rng.Next(Summaries.Length)]
                     })
                .ToArray());
            }
            catch (Exception exception)
            {
                //_logger.LogError(exception, "Something bad happened");
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public StatusCodeResult GetData()
        {
            try
            {
                var rs = ElasticConnection.Instance.client.Search<dynamic>(s => s
                .Index("elasticapi-logs-*")
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                 .Match(f => f
                                    .Field("fields.MachineName").Query("DESKTOP-GJNGD1A"))))));
                //_logger.LogInformation("Der er sku dadda");
                return new StatusCodeResult(200);
            }
            catch (Exception exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public PingReply PingServer(string customer)
        {
            Ping myPing = new Ping();
            PingReply reply = myPing.Send(customer, 1000);

            //_logger.LogInformation("Ping: {ping}", reply.RoundtripTime);
            return reply;
        }

        [HttpGet]
        public IActionResult GetDataCpu()
        {
            try
            {
                var rs = ElasticConnection.Instance.client.Search<dynamic>(s => s
                .Index("metricbeat-*")
                    .Query(q => q
                        .Bool(b => b
                            .Should(sh => sh
                                .MatchPhrase(mp => mp
                                    .Field("host.name").Query("vmi316085.contaboserver.net")
                                    .Field("event.dataset").Query("system.cpu")
                                )
                            )
                            .Filter(f => f
                                .DateRange(dr => dr
                                    .Field("@timestamp")
                                    .GreaterThanOrEquals("now-5m")
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
                if (rs.Aggregations.Count > 0)
                {
                    Dictionary<String, Object> etellerandet = new Dictionary<String, Object>();
                    var dateHistogram = rs.Aggregations.DateHistogram("myCpuDateHistogram");
                    List<Object> list = new List<Object>();

                    foreach (DateHistogramBucket item in dateHistogram.Buckets)
                    {
                        Dictionary<string, string> newlist = new Dictionary<string, string>();
                        Dictionary<string, string> cpuPair = new Dictionary<string, string>();

                        foreach (var test89 in item.Keys)
                        {
                            item.TryGetValue(test89, out IAggregate a);
                            ValueAggregate valueAggregate = a as ValueAggregate;

                            cpuPair.Add(test89, valueAggregate.Value.ToString());
                            Console.WriteLine(test89 + ": " + (valueAggregate.Value));
                            Console.WriteLine(test89 + ": " + valueAggregate.ValueAsString);
                        }
                        list.Add(cpuPair);
                        //StatsAggregate test = (StatsAggregate)item.Values.FirstOrDefault();
                        //newlist.Add("Timestamp", item.Date.ToString());
                        //newlist.Add("min", test.Min.ToString());
                        //newlist.Add("max", test.Max.ToString());
                        //newlist.Add("avg", test.Average.ToString());
                        //list.Add(newlist);
                    }

                    return Ok(JsonSerializer.Serialize(list));
                }
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetHttpErrors()
        {
            try
            {
                var rs = await ElasticConnection.Instance.client.SearchAsync<dynamic>(s => s
                .Index("packetbeat-*")
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .DateRange(r => r
                                    .Field("@timestamp")
                                    .GreaterThan("now-5m")
                                )
                            )
                        )
                    )
                    .Aggregations(aggs => aggs
                        .DateHistogram("timeseries", dh => dh
                            .Field("@timestamp")
                            .CalendarInterval("1m")
                            .Aggregations(aggs => aggs
                                .Terms("statuses", t => t
                                    .Field("status")
                                )
                            )
                        )
                    )
                );

                var timeseries = rs.Aggregations.DateHistogram("timeseries");

                if (rs.Aggregations.Count > 0)
                {
                    List<Object> list = new List<Object>();
                    foreach (var item in timeseries.Buckets.Select(s => new { s.KeyAsString, status = s.Terms("statuses") }))
                    {
                        Dictionary<string, string> newlist = new Dictionary<string, string>();
                        newlist.Add("KeyAsString", item.KeyAsString);

                        foreach (var item2 in item.status.Buckets.ToList())
                        {
                            newlist.Add(item2.Key, item2.DocCount.ToString());
                        }

                        list.Add(newlist);

                        Console.WriteLine(newlist);
                    }
                    return new ObjectResult(JsonSerializer.Serialize(list));
                }

                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetNetworkTrafic()
        {
            try
            {
                return new ObjectResult(JsonSerializer.Serialize(await _unitOfWork.Data.GetAll())) { StatusCode = 200 };
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }

            //    var response = await ElasticConnection.Instance.client.SearchAsync<dynamic>(s => s
            //    .Index("metricbeat-*")
            //    .Size(0)
            //    .Query(q => q
            //        .Bool(b => b
            //            .Should(sh => sh
            //                .MatchPhrase(mp => mp
            //                    .Field("hostname").Query("vmi316085.contaboserver.net")
            //                    .Field("event.dataset").Query("system.network")
            //                )
            //            )
            //            .Filter(f => f
            //                .DateRange(dr => dr
            //                    .Field("@timestamp")
            //                    .GreaterThanOrEquals("now-2h")
            //                    )
            //                )
            //            )
            //        )
            //    .Aggregations(aggs => aggs
            //        .DateHistogram("myNetworkDateHistogram", date => date
            //        .Field("@timestamp")
            //        .CalendarInterval(DateInterval.Minute)
            //        .Aggregations(aggs => aggs
            //            .Average("AVGnetIN", avg => avg
            //            .Field("host.network.in.bytes"))
            //            .Average("AVGnetOut", avg => avg
            //            .Field("host.network.out.bytes"))
            //            .Max("MAXnetIN", max => max
            //            .Field("host.network.in.bytes"))
            //            .Max("MAXnetOUT", max => max
            //            .Field("host.network.out.bytes"))
            //            )
            //        )
            //        )
            //    );

            //    if (response.Aggregations.Count > 0)
            //    {
            //        Console.WriteLine(response.DebugInformation);
            //        var dateHistogram = response.Aggregations.DateHistogram("myNetworkDateHistogram");
            //        List<Object> list = new List<Object>();
            //        foreach (DateHistogramBucket item in dateHistogram.Buckets)
            //        {
            //            Dictionary<string, dynamic> newlist = new Dictionary<string, dynamic>();
            //            newlist.Add("Timestamp", item.KeyAsString);

            //            foreach (var item2 in item.Keys)
            //            {
            //                item.TryGetValue(item2, out IAggregate a);
            //                ValueAggregate valueAggregate = a as ValueAggregate;
            //                newlist.Add(item2, valueAggregate.Value);
            //            }
            //            list.Add(newlist);
            //        }
            //        return Ok(JsonSerializer.Serialize(list));
            //    }
            //    return new StatusCodeResult(200);
            //}
            //catch (Exception)
            //{
            //    return new StatusCodeResult(500);
            //}
        }
        [HttpGet]
        public async Task<ActionResult> GetNetworkCpuTrafic()
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
                                    .Field("event.dataset").Query("system.network")
                                )
                            )
                            .Filter(f => f
                                .DateRange(dr => dr
                                    .Field("@timestamp")
                                    .GreaterThanOrEquals("now-2d")
                                    )
                                )
                            )
                        )
                    .Query(q => q
                        .Bool(b => b
                              .Should(sh => sh
                                    .MatchPhrase(mp => mp
                                        .Field("host.name").Query("vmi316085.contaboserver.net")
                                        .Field("event.dataset").Query("system.cpu")
                                        )
                                    )
                                .Filter(f => f
                                    .DateRange(dr => dr
                                    .Field("@timestamp")
                                    .GreaterThanOrEquals("now-2d")
                                    )
                                )
                            )
                        )
                    .Aggregations(aggs => aggs
                        .DateHistogram("myNetworkCpuDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Average("AvgUserCpu", auc => auc
                            .Field("system.cpu.user.pct"))
                            .Average("AvgSystemCpu", asc => asc
                            .Field("system.cpu.system.pct"))
                            .Max("CpuCoresMax", mcpu => mcpu
                            .Field("system.cpu.cores"))
                            .Average("AVGnetIN", avg => avg
                            .Field("host.network.in.bytes"))
                            .Average("AVGnetOut", avg => avg
                            .Field("host.network.out.bytes"))
                            .Max("MAXnetIN", max => max
                            .Field("host.network.in.bytes"))
                            .Max("MAXnetOUT", max => max
                            .Field("host.network.out.bytes"))
                        .BucketScript("CpuCalc", bs => bs
                            .BucketsPath(bp => bp
                            .Add("user", "AvgUserCpu")
                            .Add("system", "AvgSystemCpu")
                            .Add("cores", "CpuCoresMax"))
                            .Script("(params.user + params.system) / params.cores"))
                            )
                        )
                    )
                );

                if (response.Aggregations.Count > 0)
                {
                    Console.WriteLine(response.DebugInformation);
                    var dateHistogram = response.Aggregations.DateHistogram("myNetworkCpuDateHistogram");
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
                }
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }
    }
}