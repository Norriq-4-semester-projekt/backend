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

namespace WorkerService.Services
{
    internal class NetworkChangePointDetection : IHostedService, IDisposable
    {
        private int _executionCount;
        private readonly ILogger<NetworkChangePointDetection> _logger;
        private Timer _timer;

        private const string BaseDatasetsRelativePath = @"../../../../Input";
        private static readonly string DatasetRelativePath = $"{BaseDatasetsRelativePath}/network_bytes_out_trainingdata.json";
        private static string _datasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        private static MLContext _mlContext;
        private static List<Data> _trainingData = new List<Data>();
        private int _startSpikes;

        private readonly HttpClientHandler _handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private readonly HttpClient _httpClient;

        public NetworkChangePointDetection(ILogger<NetworkChangePointDetection> logger)
        {
            _httpClient = new HttpClient(_handler);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            string json = System.IO.File.ReadAllText(_datasetPath);
            List<Data> data = JsonConvert.DeserializeObject<List<Data>>(json);

            if (data != null)
            {
                foreach (var item in data)
                {
                    Data networksData = new Data();
                    networksData.Value = item.Value;
                    networksData.Timestamp = item.Timestamp;
                    _trainingData.Add(networksData);
                }
                Data emptyList = new Data();
                var spikeResult = ChangePointDetection.DetectChangepoint(emptyList, _trainingData, _startSpikes);
                _startSpikes = spikeResult.Item2.Count;

                // Create MLContext to be shared across the model creation workflow objects
                _mlContext = new MLContext();

                _logger.LogInformation("Timed Hosted Service running.");

                _timer = new Timer(DoWork, null, TimeSpan.Zero,
                    TimeSpan.FromSeconds(30));
            }
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref _executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            await DetectChangePoint();
        }

        public async Task<bool> DetectChangePoint()
        {
            bool spikeDetected = false;
            List<Data> spikes;
            try
            {
                Data latestData = new Data();
                //HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:5009/v1/SpikeDetection/GetLatestNetworkBytesOut");
                HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:5009/v2/TrainingData/GetNetworkBytesOut?interval=now-1d");

            
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                latestData = JsonConvert.DeserializeObject<Data>(responseBody);
                latestData.FieldType = "NetworkBytesOut";

                var spikeResult = ChangePointDetection.DetectChangepoint(latestData, _trainingData, _startSpikes);
                spikes = spikeResult.Item2;
                spikeDetected = spikeResult.Item1;
            }
            catch (Exception)
            {
                throw;
            }
            if (spikeDetected)
            {
                Console.WriteLine(spikeDetected);
                var telegramBot = new TelegramBotClient("1618808038:AAHs2nHXf_sYeOIgwiIr1nxqMz6Uul-w4nA");
                await telegramBot.SendTextMessageAsync("-1001399759228", "Network Changepoint Detected!\n\n" +
                    "Average Bytes: " + spikes.Last().Value + "\n" +
                    "Date: " + spikes.Last().Timestamp);
            }

            return spikeDetected;
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