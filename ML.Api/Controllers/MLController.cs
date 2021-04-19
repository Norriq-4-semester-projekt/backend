using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.TimeSeries;
using System.Net.Http;

namespace ML.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MLController : ControllerBase
    {
        private static string BaseDatasetsRelativePath = @"../../../../Data";

        private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/trainingdata.json";

        private static string DatasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        private static MLContext mlContext;
        private static List<Data> training_data = new List<Data>();

        public MLController()
        {
            if (training_data.Count > 0)
            {
            }
            else
            {
                string json = System.IO.File.ReadAllText(DatasetPath);
                List<NetworksData> data = JsonConvert.DeserializeObject<List<NetworksData>>(json);

                foreach (var item in data)
                {
                    Data networksData = new Data();
                    networksData.Bytes = item.Host.Network.In.Bytes;
                    networksData.Timestamp = item.Timestamp;
                    training_data.Add(networksData);
                }

                // Create MLContext to be shared across the model creation workflow objects
                mlContext = new MLContext();
            }
        }

        [HttpPost]
        public async Task<bool> DetectSpikeAsync(NetworksData networksData)
        {
            Data latestData = new Data();

            latestData.Bytes = networksData.Host.Network.In.Bytes;
            latestData.Timestamp = networksData.Timestamp;

            List<Data> testData = training_data;
            testData.Add(latestData);

            // Load Data
            var dataView = mlContext.Data.LoadFromEnumerable<Data>(training_data);
            //assign the Number of records in dataset file to cosntant variable
            int size = testData.Count;
            //STEP 1: Create Esimtator
            var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(NetworksDataPrediction.Prediction), inputColumnName: nameof(Data.Bytes), confidence: 95, pvalueHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            ITransformer tansformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = tansformedModel.Transform(dataView);
            IEnumerable<NetworksDataPrediction> predictions = mlContext.Data.CreateEnumerable<NetworksDataPrediction>(transformedData, reuseRowObject: false);

            List<string> spikeList = new List<string>();
            List<int> spikeDates = new List<int>();
            int i = 0;
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    spikeList.Add(p.Prediction[2].ToString());
                }
                i++;
            }

            if (spikeList.Count > 80)
            {
                return true;
            }
            else
            {
                return false;
            }
            foreach (var item in spikeDates)
            {
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