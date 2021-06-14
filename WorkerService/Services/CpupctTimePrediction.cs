using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkerService.Entities;

namespace WorkerService.Services
{
    internal class CpupctTimePrediction : IHostedService, IDisposable
    {
        private int _executionCount;
        private readonly ILogger<CpupctTimePrediction> _logger;
        private Timer _timer;

        private readonly HttpClientHandler _handler = new()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private readonly HttpClient _httpClient;

        public CpupctTimePrediction(ILogger<CpupctTimePrediction> logger)
        {
            _httpClient = new HttpClient(_handler);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
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

            await PredictDataAsync();
        }

        private async Task<float> PredictDataAsync()
        {
            TimeSpan timeSpan = new TimeSpan(0, 15, 0);
            String dateTime = DateTime.Now.TimeOfDay.Add(timeSpan).ToString();
            dateTime = dateTime.Remove(8);

            //Create single instance of sample data from first line of dataset for model input

            PredictionInputCpupct sampleData = new()
            {
                Timestamp = dateTime
            };
            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeCpupctTimeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual OutBytes with predicted OutBytes from sample data...\n\n");
            Console.WriteLine($"Time: {sampleData.Timestamp}");
            Console.WriteLine($"\n\nPredicted Cpupct: {predictionResult.Score}\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");

           

            Data predictionLog = new()
            {
                FieldType = "CpuPctPrediction",
                Value = predictionResult.Score,

                Timestamp = DateTime.Now.AddMinutes(15).ToString()
            };

            using HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var httpClient = new HttpClient(handler);
            var predictionContent = new StringContent(JsonConvert.SerializeObject(predictionLog), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("http://localhost:5001/v1/SpikeDetection/PostPredictionCpupctTime", predictionContent);
            response.EnsureSuccessStatusCode();

            return predictionResult.Score;
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