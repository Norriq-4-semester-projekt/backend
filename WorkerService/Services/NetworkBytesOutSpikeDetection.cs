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
using WorkerService.Entities.Network;

namespace WorkerService.Services
{
    internal class NetworkBytesOutSpikeDetection : IHostedService, IDisposable
    {
        private int _executionCount;
        private readonly ILogger<NetworkBytesOutSpikeDetection> _logger;
        private Timer _timer;

        private const string BaseDatasetsRelativePath = @"../../../../Input";
        private static readonly string DatasetRelativePath = $"{BaseDatasetsRelativePath}/network_bytes_out_trainingdata.json";
        private static readonly string DatasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        private static MLContext _mlContext;
        private static readonly List<Data> TrainingData = new List<Data>();
        private int _startSpikes;

        private readonly HttpClientHandler _handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private readonly HttpClient _httpClient;

        public NetworkBytesOutSpikeDetection(ILogger<NetworkBytesOutSpikeDetection> logger)
        {
            _httpClient = new HttpClient(_handler);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            List<NetworkData> data = JsonConvert.DeserializeObject<List<NetworkData>>(System.IO.File.ReadAllText(DatasetPath));
            if (data != null)
                foreach (var item in data)
                {
                    Data networksData = new Data();
                    {
                        networksData.Value = item.Host.Network.Out.Bytes;
                        networksData.Timestamp = item.Timestamp;
                    };
                    TrainingData.Add(networksData);
                }
            var spikeResult = SpikeDetection.DetectSpikeAsync(null, TrainingData, _startSpikes);

            _startSpikes = spikeResult.Item2.Count;

            // Create MLContext to be shared across the model creation workflow objects
            _mlContext = new MLContext();

            _logger.LogInformation("Timed Hosted Service running.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref _executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:5000/v1/SpikeDetection/GetLatestNetworkBytesOut");

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Data latestData = JsonConvert.DeserializeObject<Data>(responseBody);
            latestData.FieldType = "NetworkBytesOut";

            var spikeResult = SpikeDetection.DetectSpikeAsync(latestData, TrainingData, _startSpikes);
            List<Data> spikes = spikeResult.Item2;
            if (spikeResult.Item1)
            {
                var telegramBot = new TelegramBotClient("1618808038:AAHs2nHXf_sYeOIgwiIr1nxqMz6Uul-w4nA");
                await telegramBot.SendTextMessageAsync("-1001399759228", "Network Spike Detected!\n\n" +
                    "Average Bytes Out: " + spikes.Last().Value + "\n" +
                    "Date: " + spikes.Last().Timestamp);
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