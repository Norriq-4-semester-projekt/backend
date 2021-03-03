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
    }
}