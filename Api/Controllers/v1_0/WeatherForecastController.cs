using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace Api.Controllers.v1_0
{
    // [ApiVersion("0.9", Deprecated = true)] // Set previous version as deprecated
    [ApiVersion("1.0")] // Set version of controller
    [ApiController]
    [Route("v{version:apiVersion}/{customer}/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

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
                _logger.LogError(exception, "Something bad happened");
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public StatusCodeResult GetData()
        {
            try
            {
                var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("elasticapi-logs-*");
                var client = new ElasticClient(settings);

                var rs = client.Search<dynamic>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                 .Match(f => f
                                    .Field("fields.MachineName").Query("DESKTOP-GJNGD1A"))))));
                _logger.LogInformation("Der er sku dadda");
                return new StatusCodeResult(200);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public PingReply PingServer(string customer)
        {
            Ping myPing = new Ping();
            PingReply reply = myPing.Send(customer, 1000);

            _logger.LogInformation("Ping: {ping}", reply.RoundtripTime);
            return reply;
        }

        [HttpGet]
        public IActionResult GetDataCpu()
        {
            try
            {
                var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("metricbeat-*");
                settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
                settings.PrettyJson(); // Good for DEBUG
                settings.BasicAuthentication("elastic", "changeme");
                var client = new ElasticClient(settings);

                var rs = client.Search<dynamic>(s => s
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
                            .Stats("Test", st => st
                            .Field("system.cpu.user.pct")
                            )
                        )
                    )
                )
            );
                if (rs.Aggregations.Count > 0)
                {
                    Dictionary<String, Object> etellerandet = new Dictionary<String, Object>();
                    var dateHistogram = rs.Aggregations.DateHistogram("myCpuDateHistogram");
                    List<Object> list = new List<Object>();

                    foreach (var item in dateHistogram.Buckets)
                    {
                        Dictionary<string, string> newlist = new Dictionary<string, string>();
                        StatsAggregate test = (StatsAggregate)item.Values.FirstOrDefault();
                        newlist.Add("Timestamp", item.Date.ToString());
                        newlist.Add("min", test.Min.ToString());
                        newlist.Add("max", test.Max.ToString());
                        newlist.Add("avg", test.Average.ToString());
                        list.Add(newlist);
                    }

                    return Ok(JsonSerializer.Serialize(list));
                }
                return new StatusCodeResult(200);
            }
            catch (Exception exception)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public IActionResult GetHttpErrors()
        {
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("packetbeat-*");
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
            settings.BasicAuthentication("elastic", "changeme");
            var client = new ElasticClient(settings);
            try
            {
                var rs = client.Search<dynamic>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Should(sh => sh
                            .MatchPhrase(mp => mp
                                .Field("dns.question.name").Query("thekrane.dk")
                                .Field("event.dataset").Query("dns")
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
                .Aggregations(aggs => aggs
                    .DateHistogram("myDnsDataHistrogram", date => date
                    .Field("@timestamp")
                    .CalendarInterval(DateInterval.Minute)
                    //.Aggregations(aggs1 => aggs1
                    //    .ValueCount("valuecount", vc => vc
                    //    .Field("status")
                )));
                if (rs.Aggregations.Count > 0)
                {
                    Dictionary<String, Object> etellerandet = new Dictionary<String, Object>();
                    var dateHistogram = rs.Aggregations.DateHistogram("myDnsDateHistogram");
                    List<Object> list = new List<Object>();

                    foreach (var item in dateHistogram.Buckets)
                    {
                        Dictionary<String, Object> newlist = new Dictionary<String, Object>();
                        StatsAggregate test = (StatsAggregate)item.Values.FirstOrDefault();
                        newlist.Add("Timestamp", item.Date.ToString());
                        newlist.Add("OK", test.Count.Equals("OK"));
                        newlist.Add("Error", item.Count.Equals("Error"));

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
