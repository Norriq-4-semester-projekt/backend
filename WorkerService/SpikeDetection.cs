using Microsoft.ML;
using System.Collections.Generic;
using System.Linq;
using WorkerService.Entities;

namespace WorkerService
{
    public static class SpikeDetection<T> where T : class
    {
        private static MLContext mlContext = new MLContext();

        public static (bool, List<T>) DetectSpikeAsync(T latestData, List<T> trainingData, int startSpikes)
        {
            List<T> testData = new List<T>(trainingData);
            if (startSpikes > 0)
            {
                testData.Add(latestData);
            }

            // Load Data
            var dataView = mlContext.Data.LoadFromEnumerable<T>(testData);
            //assign the Number of records in dataset file to cosntant variable
            int size = testData.Count;
            //STEP 1: Create Esimtator
            var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(Predictions.Prediction), inputColumnName: "Value", confidence: 99, pvalueHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            ITransformer transformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = transformedModel.Transform(dataView);
            IEnumerable<Predictions> predictions = mlContext.Data.CreateEnumerable<Predictions>(transformedData, reuseRowObject: false);

            List<string> spikeList = new List<string>();
            List<T> spikes = new List<T>();
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
                return (true, spikes);
            }
            else
            {
                return (false, spikes);
            }
        }

        private static IDataView CreateEmptyDataView()
        {
            //Create empty DataView. We just need the schema to call fit()
            IEnumerable<T> enumerableData = new List<T>();
            var dv = mlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }
    }
}