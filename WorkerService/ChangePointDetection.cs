using Microsoft.ML;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using WorkerService.Entities;

namespace WorkerService
{
    public static class ChangePointDetection
    {
        private static readonly MLContext mlContext = new();
        private static bool firstRun = true;

        public static (bool, List<Data>) DetectChangepoint(Data latestData, List<Data> trainingData, int startChangepoints)
        {
            List<Data> testData = new(trainingData);
            if (latestData != null)
            {
                testData.Add(latestData);
                firstRun = false;
            }
            // Load Data
            var dataView = mlContext.Data.LoadFromEnumerable<Data>(testData);
            //assign the Number of records in dataset file to variable
            int size = testData.Count;
            var estimator = mlContext.Transforms.DetectIidChangePoint(outputColumnName: nameof(Predictions.Prediction), inputColumnName: "Value", confidence: 75, changeHistoryLength: size / 4);

            ITransformer tansformedModel = estimator.Fit(CreateEmptyDataView());

            // Use/test model
            IDataView transformedData = tansformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<Predictions>(transformedData, reuseRowObject: false);

            List<Data> changepoints = new();
            int i = 0;
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    changepoints.Add(testData.ElementAt(i));
                }
                i++;
            }

            if (changepoints.Count > startChangepoints)
            {
                changepoints.Last().IsSpike = true;
                System.Console.WriteLine(changepoints.Last().IsSpike);
                LogDataAsync(changepoints.Last());

                return (true, changepoints);
            }
            else
            {
                LogDataAsync(latestData);

                return (false, changepoints);
            }
        }

        private static async void LogDataAsync(Data data)
        {
            if (!firstRun)
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    using (var httpClient = new HttpClient(handler))
                    {
                        var StringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                        HttpResponseMessage response2 = await httpClient.PostAsync("http://localhost:5010/v1/SpikeDetection/PostChangepointData", StringContent);
                        response2.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        private static IDataView CreateEmptyDataView()
        {
            //Create empty DataView. We just need the schema to call fit()
            IEnumerable<Data> enumerableData = new List<Data>();
            var dv = mlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }
    }
}