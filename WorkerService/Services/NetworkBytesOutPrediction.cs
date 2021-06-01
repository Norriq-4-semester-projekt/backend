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
    internal class NetworkBytesOutPrediction : IHostedService, IDisposable
    {
        private int _executionCount;
        private readonly ILogger<NetworkBytesOutPrediction> _logger;
        private Timer _timer;

        private readonly HttpClientHandler _handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        private readonly HttpClient _httpClient;

        public NetworkBytesOutPrediction(ILogger<NetworkBytesOutPrediction> logger)
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
            HttpResponseMessage cpuResponse = await _httpClient.GetAsync("https://localhost:5009/v1/SpikeDetection/GetLatestCpuData");
            cpuResponse.EnsureSuccessStatusCode();
            string cpuResponseBody = await cpuResponse.Content.ReadAsStringAsync();
            Data cpuData = JsonConvert.DeserializeObject<Data>(cpuResponseBody);

            HttpResponseMessage memResponse = await _httpClient.GetAsync("https://localhost:5009/v1/SpikeDetection/GetLatestMemoryData");
            memResponse.EnsureSuccessStatusCode();
            string memResponseBody = await memResponse.Content.ReadAsStringAsync();
            Data memoryData = JsonConvert.DeserializeObject<Data>(memResponseBody);

            HttpResponseMessage bytesInResponse = await _httpClient.GetAsync("https://localhost:5009/v1/SpikeDetection/GetLatestNetworkBytesIn");
            bytesInResponse.EnsureSuccessStatusCode();
            string bytesInResponseBody = await bytesInResponse.Content.ReadAsStringAsync();
            Data bytesInData = JsonConvert.DeserializeObject<Data>(bytesInResponseBody);

            HttpResponseMessage systemLoadResponse = await _httpClient.GetAsync("https://localhost:5009/v1/SpikeDetection/GetLatestSystemLoadData");
            systemLoadResponse.EnsureSuccessStatusCode();
            string systemLoadResponseBody = await systemLoadResponse.Content.ReadAsStringAsync();
            Data systemLoadData = JsonConvert.DeserializeObject<Data>(systemLoadResponseBody);

            HttpResponseMessage bytesOutResponse = await _httpClient.GetAsync("https://localhost:5009/v1/SpikeDetection/GetLatestNetworkBytesOut");
            bytesOutResponse.EnsureSuccessStatusCode();
            string bytesOutResponseBody = await bytesOutResponse.Content.ReadAsStringAsync();
            Data bytesOutData = JsonConvert.DeserializeObject<Data>(bytesOutResponseBody);

            //Create single instance of sample data from first line of dataset for model input

            PredictionInput sampleData = new PredictionInput()
            {
                Number = systemLoadData.Value,
                MemBytes = memoryData.Value,
                InBytes = bytesInData.Value,
                CpuPct = cpuData.Value,
            };
            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual OutBytes with predicted OutBytes from sample data...\n\n");
            Console.WriteLine($"Number: {sampleData.Number}");
            Console.WriteLine($"MemBytes: {sampleData.MemBytes}");
            Console.WriteLine($"InBytes: {sampleData.InBytes}");
            Console.WriteLine($"CpuPct: {sampleData.CpuPct}");
            Console.WriteLine($"\n\nPredicted OutBytes: {predictionResult.Score}\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");

            Data predictionLog = new Data()
            {
                FieldType = "BytesOutPrediction",
                Value = predictionResult.Score,
                Timestamp = bytesOutData.Timestamp
            };

            Data bytesOutLog = new Data()
            {
                FieldType = "BytesOutActual",
                Value = bytesOutData.Value,
                Timestamp = bytesOutData.Timestamp
            };

            using HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var httpClient = new HttpClient(handler);
            var predictionContent = new StringContent(JsonConvert.SerializeObject(predictionLog), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("http://localhost:5010/v1/SpikeDetection/PostPredictionData", predictionContent);
            response.EnsureSuccessStatusCode();

            var actualContent = new StringContent(JsonConvert.SerializeObject(bytesOutLog), Encoding.UTF8, "application/json");
            HttpResponseMessage response2 = await httpClient.PostAsync("http://localhost:5010/v1/SpikeDetection/PostPredictionData", actualContent);
            response2.EnsureSuccessStatusCode();

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