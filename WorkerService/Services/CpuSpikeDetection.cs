using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using WorkerService.Entities;
using WorkerService.Entities.Cpu;

namespace WorkerService.Services
{
    internal class CpuSpikeDetection : IHostedService, IDisposable
    {
        private int _executionCount;
        private readonly ILogger<CpuSpikeDetection> _logger;
        private Timer _timer;

        private const string BaseDatasetsRelativePath = @"../../../../Input";
        private static readonly string DatasetRelativePath = $"{BaseDatasetsRelativePath}/cpu_trainingdata.json";
        private static readonly string DatasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        private static readonly List<Data> TrainingData = new();
        private int _startSpikes;

        private readonly HttpClientHandler _handler = new()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private readonly HttpClient httpClient;

        public CpuSpikeDetection(ILogger<CpuSpikeDetection> logger)
        {
            httpClient = new HttpClient(_handler);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            string json = System.IO.File.ReadAllText(DatasetPath);
            List<CpuData> data = JsonConvert.DeserializeObject<List<CpuData>>(json);

            if (data != null)
                foreach (var item in data)
                {
                    Data cpuData = new()
                    {
                        Value = item.Host.Cpu.Pct,
                        Timestamp = item.Timestamp
                    };
                    TrainingData.Add(cpuData);
                }

            Data emptyList = new();
            var spikeResult = SpikeDetection.DetectSpikeAsync(emptyList, TrainingData, _startSpikes);
            _startSpikes = spikeResult.Item2.Count;

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

        public async Task<bool> DetectSpikeAsync()
        {
            Data latestData;
            HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5000/v1/SpikeDetection/GetLatestCpuData");

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            latestData = JsonConvert.DeserializeObject<Data>(responseBody);
            latestData.FieldType = "CpuPct";

            var spikeResult = SpikeDetection.DetectSpikeAsync(latestData, TrainingData, _startSpikes);
            Data spike = spikeResult.Item2.Last();
            if (spikeResult.Item1)
            {
                var telegramBot = new TelegramBotClient("1618808038:AAHs2nHXf_sYeOIgwiIr1nxqMz6Uul-w4nA");
                await telegramBot.SendTextMessageAsync("-1001399759228", "CPU Spike Detected!\n\n" +
                    "CPU % Used: " + spike.Value + "\n" +
                    "Date: " + spike.Timestamp);
            }

            return spikeResult.Item1;
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