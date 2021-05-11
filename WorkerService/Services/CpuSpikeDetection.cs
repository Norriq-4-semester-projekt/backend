using DataAccess.Entities.Cpu;
using DataAccess.Entities.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using WorkerService.Entities;

namespace WorkerService.Services
{
    internal class CpuSpikeDetection : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<CpuSpikeDetection> _logger;
        private Timer _timer;

        private static string BaseDatasetsRelativePath = @"../../../../Input";
        private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/cpu_trainingdata.json";
        private static string DatasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        private static MLContext mlContext;
        private static List<Data> trainingData = new List<Data>();
        private int startSpikes = 0;

        private HttpClientHandler handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private HttpClient httpClient;

        public CpuSpikeDetection(ILogger<CpuSpikeDetection> logger)
        {
            httpClient = new HttpClient(handler);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            string json = System.IO.File.ReadAllText(DatasetPath);
            List<CpuData> data = JsonConvert.DeserializeObject<List<CpuData>>(json);

            foreach (var item in data)
            {
                Data cpuData = new Data();
                cpuData.Value = item.Host.Cpu.Pct;
                cpuData.Timestamp = item.Timestamp;
                trainingData.Add(cpuData);
            }
            Data emptyList = new Data();
            var spikeResult = SpikeDetection.DetectSpikeAsync(emptyList, trainingData, startSpikes);
            startSpikes = spikeResult.Item2.Count;

            // Create MLContext to be shared across the model creation workflow objects
            mlContext = new MLContext();

            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

            await DetectSpikeAsync();
        }

        public async Task<bool> DetectSpikeAsync()
        {
            bool spikeDetected = false;
            Data spike;
            try
            {
                Data latestData = new Data();
                HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/v1/SpikeDetection/GetLatestCpuData");

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                latestData = JsonConvert.DeserializeObject<Data>(responseBody);
                latestData.FieldType = "CpuPct";

                var spikeResult = SpikeDetection.DetectSpikeAsync(latestData, trainingData, startSpikes);
                spike = spikeResult.Item2.Last();
                spikeDetected = spikeResult.Item1;
            }
            catch (Exception)
            {
                throw;
            }
            if (spikeDetected)
            {
                var telegramBot = new TelegramBotClient("1618808038:AAHs2nHXf_sYeOIgwiIr1nxqMz6Uul-w4nA");
                await telegramBot.SendTextMessageAsync("-1001399759228", "CPU Spike Detected!\n\n" +
                    "CPU % Used: " + spike.Value + "\n" +
                    "Date: " + spike.Timestamp);
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