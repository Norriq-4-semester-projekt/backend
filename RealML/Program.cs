using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealML
{
    internal class Program
    {
        private List<AllData> dataList = new List<AllData>();
        private AllData allData = new AllData();

        private static void Main(string[] args)
        {
            var context = new MLContext();

            var data = context.Data.LoadFromTextFile<NetworkData>("./rs.csv", hasHeader: true,
                separatorChar: ',');

            var split = context.Data.TrainTestSplit(data, testFraction: 0.2);

            var features = split.TrainSet.Schema
                .Select(col => col.Name)
                .Where(colName => colName != "Label" && colName != "Timestamp")
                .ToArray();

            var pipeline = context.Transforms.Text.FeaturizeText("timestamp", "Timestamp")
                .Append(context.Transforms.Concatenate("Features", features))
                .Append(context.Transforms.Concatenate("Feature", "Features", "timestamp"))
                .Append(context.Regression.Trainers.LbfgsPoissonRegression());

            //ConsoleHelper.PeekDataViewInConsole(context, trainingDataView, dataProcessPipeline, 5);
            //ConsoleHelper.PeekVectorColumnDataInConsole(context, "Features", trainingDataView, dataProcessPipeline, 5);

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);

            var metrics = context.Regression.Evaluate(predictions);

            Console.WriteLine($"R^2 - {metrics.RSquared}");
        }
    }
}