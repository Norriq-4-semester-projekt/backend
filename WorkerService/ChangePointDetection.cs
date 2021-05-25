﻿using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerService.entities;
using WorkerService.Entities;

namespace WorkerService
{
    public static class ChangePointDetection
    {
        private static MLContext mlContext = new MLContext();
        private static bool _firstRun = true;

        public static (bool, List<Data>) DetectChangepoint(Data latestData, List<Data> trainingData, int startSpikes)
        {
            List<Data> testData = new List<Data>(trainingData);
            if (startSpikes > 0)
            {
                _firstRun = false;
                testData.Add(latestData);
            }
            // Load Data
            var dataView = mlContext.Data.LoadFromEnumerable<Data>(testData);
            //assign the Number of records in dataset file to cosntant variable
            int size = testData.Count;
            //STEP 1: Setup transformations using DetectIidChangePoint
            var estimator = mlContext.Transforms.DetectIidChangePoint(outputColumnName: nameof(Predictions.Prediction), inputColumnName: "Value", confidence: 90, changeHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            //In IID Change point detection, we don't need need to do training, we just need to do transformation.
            //As you are not training the model, there is no need to load IDataView with real data, you just need schema of data.
            //So create empty data view and pass to Fit() method.
            ITransformer tansformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = tansformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<Predictions>(transformedData, reuseRowObject: false);

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

            if (spikes.Count > startSpikes)
            {
                spikes.Last().IsSpike = true;
                return (true, spikes);
            }
            return (false, spikes);
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