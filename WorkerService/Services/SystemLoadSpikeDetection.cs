using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using WorkerService.Entities;
using WorkerService.Entities.Load;

namespace WorkerService.Services
{
    internal class SystemLoadSpikeDetection : IHostedService, IDisposable
    {
        private int _executionCount;
        private readonly ILogger<SystemLoadSpikeDetection> _logger;
        private Timer _timer;

        private const string BaseDatasetsRelativePath = @"../../../../Input";
        private static readonly string DatasetRelativePath = $"{BaseDatasetsRelativePath}/systemload_trainingdata.json";
        private static readonly string DatasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        private static MLContext _mlContext;
        private static readonly List<Data> TrainingData = new List<Data>();
        private int _startSpikes;

        private readonly HttpClientHandler _handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private readonly HttpClient _httpClient;

        public SystemLoadSpikeDetection(ILogger<SystemLoadSpikeDetection> logger)
        {
            _httpClient = new HttpClient(_handler);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            string json = System.IO.File.ReadAllText(DatasetPath);
            List<SystemLoadData> data = JsonConvert.DeserializeObject<List<SystemLoadData>>(json);

            if (data != null)
                foreach (var item in data)
                {
                    Data systemloadData = new Data();
                    {
                        systemloadData.Value = item.System.Load.Number;
                        systemloadData.Timestamp = item.Timestamp;
                    };
                    TrainingData.Add(systemloadData);
                }

            Data emptyList = new Data();
            var spikeResult = SpikeDetection.DetectSpikeAsync(emptyList, TrainingData, _startSpikes);
            _startSpikes = spikeResult.Item2.Count;

            // Create MLContext to be shared across the model creation workflow objects
            _mlContext = new MLContext();

            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref _executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            await DetectSpikeAsync();
        }

        private async Task<bool> DetectSpikeAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:5000/v1/SpikeDetection/GetLatestSystemLoadData");

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Data latestData = JsonConvert.DeserializeObject<Data>(responseBody);
            latestData.FieldType = "SystemLoad";

            var (item1, spikes) = SpikeDetection.DetectSpikeAsync(latestData, TrainingData, _startSpikes);
            if (item1)
            {
                var telegramBot = new TelegramBotClient("1618808038:AAHs2nHXf_sYeOIgwiIr1nxqMz6Uul-w4nA");
                await telegramBot.SendTextMessageAsync("-1001399759228", "System Load Spike Detected!\n\n" +
                    "Average System Load 15: " + spikes.Last().Value + "\n" +
                    "Date: " + spikes.Last().Timestamp);
            }

            return item1;
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