using Microsoft.ML;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using WorkerService.Entities;

namespace WorkerService
{
    public static class SpikeDetection
    {
        private static readonly MLContext MlContext = new();

        public static (bool, List<Data>) DetectSpikeAsync(Data latestData, List<Data> trainingData, int startSpikes)
        {
            List<Data> testData = new(trainingData);
            if (latestData != null)
            {
                testData.Add(latestData);
            }

            // Load Data
            var dataView = MlContext.Data.LoadFromEnumerable<Data>(testData);
            //assign the Number of records in dataset file to cosntant variable
            int size = testData.Count;
            //STEP 1: Create Estimator
            var estimator = MlContext.Transforms.DetectIidSpike(outputColumnName: nameof(Predictions.Prediction),
                                                                inputColumnName: "Value",
                                                                confidence: 99,
                                                                pvalueHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            ITransformer transformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = transformedModel.Transform(dataView);
            IEnumerable<Predictions> predictions = MlContext.Data.CreateEnumerable<Predictions>(transformedData, reuseRowObject: false);

            List<string> spikeList = new();
            List<Data> spikes = new();
            int i = 0;
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    spikes.Add(testData.ElementAt(i));
                    spikeList.Add(p.Prediction[2].ToString());
                }
                i++;
            }

            if (spikeList.Count > startSpikes)
            {
                spikes.Last().IsSpike = true;
                LogDataAsync(spikes.Last());
                return (true, spikes);
            }

            LogDataAsync(latestData);
            return (false, spikes);
        }

        private static async void LogDataAsync(Data data)
        {
            if (!_firstRun)
            {
                using HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var httpClient = new HttpClient(handler);
                var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("http://localhost:5010/v1/SpikeDetection/PostDetectionData", stringContent);
                //response.EnsureSuccessStatusCode();
            }
        }

        private static IDataView CreateEmptyDataView()
        {
            //Create empty DataView. We just need the schema to call fit()
            IEnumerable<Data> enumerableData = new List<Data>();
            var dv = MlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }
    }
}