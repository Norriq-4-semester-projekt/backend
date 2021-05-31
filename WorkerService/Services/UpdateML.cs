using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService.Services
{
    internal class UpdateMl : IHostedService, IDisposable
    {
        private int _executionCount;
        private readonly ILogger<UpdateMl> _logger;
        private Timer _timer;

        private const string BaseDatasetsRelativePath = @"../../../../Input";

        public UpdateMl(ILogger<UpdateMl> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromDays(7));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref _executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            await UpdateTrainingModel();
        }

        private static async Task UpdateTrainingModel()
        {
            Dictionary<string, string> endPoints = new Dictionary<string, string>
            {
                {"cpu_trainingdata", "https://localhost:5001/v1/TrainingData/GetCpuData"},
                {"network_bytes_in_trainingdata", "https://localhost:5001/v1/TrainingData/GetNetworkBytesIn"},
                {"memory_trainingdata", "https://localhost:5001/v1/TrainingData/GetMemoryData"},
                {"network_bytes_out_trainingdata", "https://localhost:5001/v1/TrainingData/GetNetworkBytesOut"},
                {"systemload_trainingdata", "https://localhost:5001/v1/TrainingData/GetSystemLoadData"}
            };
            Dictionary<string, string> responses = new Dictionary<string, string>();
            foreach (var (key, value) in endPoints)
            {
                responses.Add(key, PathHelper.GetJsonResponse(value).Result);
            }
            foreach (var item in responses)
            {
                string datasetRelativePath = $"{BaseDatasetsRelativePath}/{item.Key}.json";
                string oldLocation = PathHelper.GetAbsolutePath(datasetRelativePath);
                string backupName = item.Key + "_BACKUP";
                datasetRelativePath = $"{BaseDatasetsRelativePath}/{backupName}.json";
                string newLocation = PathHelper.GetAbsolutePath(datasetRelativePath);
                File.Delete(newLocation);
                File.Move(oldLocation, newLocation);
                await File.WriteAllTextAsync(oldLocation, item.Value);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}