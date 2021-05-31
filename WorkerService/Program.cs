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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<NetworkBytesOutSpikeDetection>();
                    //services.AddHostedService<NetworkBytesInSpikeDetection>();
                    //services.AddHostedService<MemorySpikeDetection>();
                    //services.AddHostedService<SystemLoadSpikeDetection>();
                    //services.AddHostedService<CpuSpikeDetection>();
                    //services.AddHostedService<UpdateMl>();
                    //services.AddHostedService<NetworkBytesOutPrediction>();
                    services.AddHostedService<NetworkChangePointDetection>();
                    services.AddHostedService<Worker>();
                });
                
    }
}