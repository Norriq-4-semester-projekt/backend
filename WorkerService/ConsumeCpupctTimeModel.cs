// This file was auto-generated by ML.NET Model Builder.

using Microsoft.ML;
using System;
using WorkerService.Entities;

namespace WorkerService
{
    public static class ConsumeCpupctTimeModel
    {
        private const string BaseDatasetsRelativePath = @"../../../../Input/Prediction";
        private static readonly string DatasetRelativePath = $"{BaseDatasetsRelativePath}/CpupctTime.zip";
        private static readonly string DatasetPath = PathHelper.GetAbsolutePath(DatasetRelativePath);

        private static readonly Lazy<PredictionEngine<PredictionInputCpupct, PredictionOutput>> PredictionEngine = new(CreatePredictionEngine);

        // For more info on consuming ML.NET models, visit https://aka.ms/mlnet-consume
        // Method for consuming model in your app
        public static PredictionOutput Predict(PredictionInputCpupct input)
        {
            PredictionOutput result = PredictionEngine.Value.Predict(input);
            return result;
        }

        public static PredictionEngine<PredictionInputCpupct, PredictionOutput> CreatePredictionEngine()
        {
            // Create new MLContext
            MLContext mlContext = new();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(DatasetPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PredictionInputCpupct, PredictionOutput>(mlModel);

            return predEngine;
        }
    }
}