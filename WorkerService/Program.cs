using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkerService.Services;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<NetworkBytesOutSpikeDetection>();
                    services.AddHostedService<NetworkBytesInSpikeDetection>();
                    services.AddHostedService<MemorySpikeDetection>();
                    services.AddHostedService<SystemLoadSpikeDetection>();
                    services.AddHostedService<CpuSpikeDetection>();
                    services.AddHostedService<UpdateML>();
                    //services.AddHostedService<NetworkChangePointDetection>();

                    services.AddHostedService<Worker>();
                });
    }
}