using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration()
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .CreateLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog(configureLogger: (context, configuration) =>
             {
                 configuration.Enrich.FromLogContext()
                 .Enrich.WithMachineName()
                 .WriteTo.Console()
                 .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(node: new Uri(context.Configuration["ElasticConfiguration:Uri"]))
                 {
                     IndexFormat = $"{context.Configuration["ApplicationName"]}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(oldValue: ".", newValue: "-")}-{DateTime.UtcNow:yyyy-MM}",
                     AutoRegisterTemplate = true,
                     NumberOfShards = 2,
                     NumberOfReplicas = 1
                 })
                 .Enrich.WithProperty(name: "Environment", context.HostingEnvironment.EnvironmentName)
                 .ReadFrom.Configuration(context.Configuration);
             })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}