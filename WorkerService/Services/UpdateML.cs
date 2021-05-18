using DataAccess.Entities.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using WorkerService.Entities;

namespace WorkerService.Services
{
    internal class UpdateML : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<UpdateML> _logger;
        private Timer _timer;

        private static string FileName;
        private static string BaseDatasetsRelativePath = @"../../../../Input";
        //private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/{FileName}.json";
        //private static string DatasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        public UpdateML(ILogger<UpdateML> logger)
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
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            await UpdateTrainingModel();
        }

        private async Task UpdateTrainingModel()
        {
            Dictionary<String, String> endPoints = new Dictionary<string, string>();
            endPoints.Add("cpu_trainingdata", "https://localhost:5001/v1/TrainingData/GetCpuData");
            endPoints.Add("network_bytes_in_trainingdata", "https://localhost:5001/v1/TrainingData/GetNetworkBytesIn");
            endPoints.Add("memory_trainingdata", "https://localhost:5001/v1/TrainingData/GetMemoryData");
            endPoints.Add("network_bytes_out_trainingdata", "https://localhost:5001/v1/TrainingData/GetNetworkBytesOut");
            endPoints.Add("systemload_trainingdata", "https://localhost:5001/v1/TrainingData/GetSystemLoadData");
            Dictionary<String, String> responses = new Dictionary<string, string>();

            foreach (var item in endPoints)
            {
                responses.Add(item.Key, PathHelper.GetJsonResponse(item.Value).Result);
            }

            foreach (var item in responses)
            {
                string DatasetRelativePath = $"{BaseDatasetsRelativePath}/{item.Key}.json";
                string oldLocation = PathHelper.GetAbsolutePath(DatasetRelativePath);

                string backupName = item.Key + "_BACKUP";
                DatasetRelativePath = $"{BaseDatasetsRelativePath}/{backupName}.json";
                string newLocation = PathHelper.GetAbsolutePath(DatasetRelativePath);

                File.Delete(newLocation);
                File.Move(oldLocation, newLocation);

                File.WriteAllText(oldLocation, item.Value);
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