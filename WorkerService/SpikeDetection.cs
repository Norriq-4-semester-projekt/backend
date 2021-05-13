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
        private static MLContext mlContext = new MLContext();
        private static bool firstRun = true;

        private static HttpClientHandler handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        public static (bool, List<Data>) DetectSpikeAsync(Data latestData, List<Data> trainingData, int startSpikes)
        {
            List<Data> testData = new List<Data>(trainingData);
            if (startSpikes > 0)
            {
                firstRun = false;
                testData.Add(latestData);
            }

            // Load Data
            var dataView = mlContext.Data.LoadFromEnumerable<Data>(testData);
            //assign the Number of records in dataset file to cosntant variable
            int size = testData.Count;
            //STEP 1: Create Esimtator
            var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(Predictions.Prediction),
                                                                inputColumnName: "Value",
                                                                confidence: 99,
                                                                pvalueHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            ITransformer transformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = transformedModel.Transform(dataView);
            IEnumerable<Predictions> predictions = mlContext.Data.CreateEnumerable<Predictions>(transformedData, reuseRowObject: false);

            List<string> spikeList = new List<string>();
            List<Data> spikes = new List<Data>();
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
            else
            {
                LogDataAsync(latestData);
                return (false, spikes);
            }
        }

        private static async void LogDataAsync(Data data)
        {
            if (!firstRun)
            {
                using (var httpClient = new HttpClient(handler)) {

                    var StringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    HttpResponseMessage response2 = await httpClient.PostAsync("https://localhost:5001/v1/SpikeDetection/PostDetectionData", StringContent);
                    response2.EnsureSuccessStatusCode();
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