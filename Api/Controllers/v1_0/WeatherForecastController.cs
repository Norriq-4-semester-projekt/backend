using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Linq;
using System.Net.NetworkInformation;

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
        public StatusCodeResult GetDataCpu()
        {
            try
            {
                var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("metricbeat-*");
                settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
                settings.PrettyJson(); // Good for DEBUG
                var client = new ElasticClient(settings);

                var rs = client.Search<dynamic>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Should(sh => sh
                                .MatchPhrase(mp => mp
                                    .Field("host.name")
                                    .Query("vmi316085.contaboserver.net")
                                    .Field("event.dataset")
                                    .Query("system.cpu")

                                )
                            )

                            .Filter(f => f
                                .DateRange(r => r
                                    .Field("@timestamp")
                                    .GreaterThanOrEquals("now-10m")
                                    )
                                )
                            )
                        )
                    .Aggregations(aggs1 => aggs1
                        .DateHistogram("myDateHistogram", date => date
                        .Field("@timestamp")
                        .CalendarInterval(DateInterval.Minute)
                        .Aggregations(aggs => aggs
                            .Max("mySampledCpuUserMax", max => max
                                .Field("system.cpu.user.pct")
                                )
                            .Min("mySampledCpuUserMin", min => min
                                .Field("system.cpu.user.pct")
                                )
                            .Average("mySampledCpuUserAvg", avg => avg
                                .Field("system.cpu.user.pct")

                        )
                    ))));
                if (rs.Aggregations.Count > 0)
                {
                    var response = rs.Aggregations.DateHistogram("myDateHistogram");
                    var dateHistogram = rs.Aggregations.DateHistogram("myDateHistogram");

                    //var children = rs.Aggregations.Max("mySampleCpuUserMax");
                    foreach (var items in dateHistogram.Buckets)
                    {
                        Console.WriteLine(dateHistogram.Buckets);
                        Console.WriteLine(items);
                        Console.WriteLine(response);

                    }
                    //Console.WriteLine(aggs);
                    return new StatusCodeResult(200);
                }

                return Ok();
            }
            catch (Exception exception)
            {
                return new StatusCodeResult(500);
            }
        }
    }
}