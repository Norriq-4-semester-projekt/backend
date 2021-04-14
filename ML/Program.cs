using Microsoft.ML;
using Microsoft.ML.Data;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ML
{
    public class Program
    {
        private static string BaseDatasetsRelativePath = @"..\..\..\Data";

        private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/rs.csv";
        //private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/response.json";

        //private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/http.csv";

        private static string DatasetPath = GetAbsolutePath(DatasetRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/ProductSalesModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        private static MLContext mlContext;

        public static async System.Threading.Tasks.Task Main()
        {
            //DataContractJsonSerializer dj = new DataContractJsonSerializer(typeof(ProductsDataList));
            //MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(await File.OpenText(DatasetRelativePath).ReadToEndAsync()));
            //ProductsDataList training_data = (ProductsDataList)dj.ReadObject(ms);

            //ProductsDataList training_data = UpdateMLModel();
            //NetworksDataList training_data = Network();

            // Create MLContext to be shared across the model creation workflow objects
            mlContext = new MLContext();

            // Load Data
            var dataView = mlContext.Data.LoadFromTextFile<NetworksData>(
            DatasetPath,
            separatorChar: ',',
            hasHeader: true);

            //assign the Number of records in dataset file to cosntant variable
            //int size = training_data.Data.Count;

            //Load the data into IDataView.
            //This dataset is used while prediction/detecting spikes or changes.

            //IDataView dataView = mlContext.Data.LoadFromTextFile<ProductSalesData>(path: DatasetPath, hasHeader: true, separatorChar: ',');
            //IDataView dataView = mlContext.Data.LoadFromEnumerable(training_data.Data);

            //To detech temporay changes in the pattern
            //DetectSpike(size, dataView);

            //To detect persistent change in the pattern
            //DetectChangepoint(size, dataView);

            //Console.WriteLine("=============== End of process, hit any key to finish ===============");

            //Console.ReadLine();
        }

        public Program()
        {
            // Create MLContext to be shared across the model creation workflow objects
            mlContext = new MLContext();

            // Load Data
            var dataView = mlContext.Data.LoadFromTextFile<NetworksData>(
            DatasetPath,
            separatorChar: ',',
            hasHeader: true);
        }

        private static void DetectSpike(int size, IDataView dataView)
        {
            Console.WriteLine("===============Detect temporary changes in pattern===============");

            ////STEP 1: Create Esimtator
            ////var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(ProductSalesPrediction.Prediction), inputColumnName: nameof(ProductSalesData.doc_count), confidence: 99, pvalueHistoryLength: size / 4);
            ////var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(ProductSalesPrediction.Prediction), inputColumnName: nameof(ProductSalesData.doc_count), confidence: 99, pvalueHistoryLength: size / 4);
            var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(NetworksDataPrediction.Prediction), inputColumnName: nameof(NetworksData.Host__Network__In__Bytes), confidence: 95, pvalueHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            //In IID Spike detection, we don't need to do training, we just need to do transformation.
            //As you are not training the model, there is no need to load IDataView with real data, you just need schema of data.
            //So create empty data view and pass to Fit() method.
            ITransformer tansformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = tansformedModel.Transform(dataView);
            //var predictions = mlContext.Data.CreateEnumerable<NetworksDataPrediction>(transformedData, reuseRowObject: false);
            IEnumerable<NetworksDataPrediction> predictions = mlContext.Data.CreateEnumerable<NetworksDataPrediction>(transformedData, reuseRowObject: false);

            Console.WriteLine("Alert\tScore\tP-Value");
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}", p.Prediction[0], p.Prediction[1], p.Prediction[2]);
                Console.ResetColor();
            }
            Console.WriteLine("");
        }

        private static void DetectChangepoint(int size, IDataView dataView)
        {
            Console.WriteLine("===============Detect Persistent changes in pattern===============");

            //STEP 1: Setup transformations using DetectIidChangePoint
            //var estimator = mlContext.Transforms.DetectIidChangePoint(outputColumnName: nameof(ProductSalesPrediction.Prediction), inputColumnName: nameof(ProductSalesData.doc_count), confidence: 95, changeHistoryLength: size / 4);
            var estimator = mlContext.Transforms.DetectIidChangePoint(outputColumnName: nameof(NetworksDataPrediction.Prediction), inputColumnName: nameof(NetworksData.Host__Network__In__Bytes), confidence: 95, changeHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            //In IID Change point detection, we don't need need to do training, we just need to do transformation.
            //As you are not training the model, there is no need to load IDataView with real data, you just need schema of data.
            //So create empty data view and pass to Fit() method.
            ITransformer tansformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = tansformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<NetworksDataPrediction>(transformedData, reuseRowObject: false);

            Console.WriteLine($"{nameof(NetworksDataPrediction.Prediction)} column obtained post-transformation.");
            Console.WriteLine("Alert\tScore\tP-Value\tvalue");

            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}  <-- alert is on, predicted changepoint", p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);
                }
                else
                {
                    Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}", p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);
                }
            }
            Console.WriteLine("");
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        private static IDataView CreateEmptyDataView()
        {
            //Create empty DataView. We just need the schema to call fit()
            IEnumerable<NetworksData> enumerableData = new List<NetworksData>();
            var dv = mlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }

        public static NetworksDataList Network()
        {
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("metricbeat-*");
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
            settings.BasicAuthentication("elastic", "changeme");
            var client = new ElasticClient(settings);

            var response = client.Search<dynamic>(s => s
               .Size(0)
               .Query(q => q
                   .Bool(b => b
                       .Should(sh => sh
                           .MatchPhrase(mp => mp
                               .Field("hostname").Query("vmi316085.contaboserver.net")
                               .Field("event.dataset").Query("system.network")
                           )
                       )
                       .Filter(f => f
                           .DateRange(dr => dr
                               .Field("@timestamp")
                               .GreaterThanOrEquals("now-1h")
                               )
                           )
                       )
                   )
               .Aggregations(aggs => aggs
                   .DateHistogram("myNetworkDateHistogram", date => date
                   .Field("@timestamp")
                   .CalendarInterval(DateInterval.Minute)
                   .Aggregations(aggs => aggs
                       //.Average("AVGnetIN", avg => avg
                       //.Field("host.network.in.bytes"))
                       //.Average("AVGnetOut", avg => avg
                       //.Field("host.network.out.bytes"))
                       .Max("MAXnetIN", max => max
                       .Field("host.network.in.bytes"))
                       //.Max("MAXnetOUT", max => max
                       //.Field("host.network.out.bytes"))
                       )
                   )
                   )
                );

            NetworksDataList list = new NetworksDataList();
            list.Data = new List<NetworksData>();
            if (response.Aggregations.Count > 0)
            {
                foreach (DateHistogramBucket item in response.Aggregations.DateHistogram("myNetworkDateHistogram").Buckets)
                {
                    NetworksData nd = new NetworksData();
                    Dictionary<string, double?> netwirk = new Dictionary<string, double?>();
                    foreach (var test in item.Keys)
                    {
                        item.TryGetValue(test, out IAggregate a);
                        ValueAggregate valueAggregate = a as ValueAggregate;
                        nd.Host__Network__In__Bytes = (float)valueAggregate.Value;
                        nd.Timestamp = item.KeyAsString;
                        netwirk.Add(test, valueAggregate.Value);
                        Console.WriteLine(test + ": " + valueAggregate.Value);
                    }

                    list.Data.Add(nd);
                }
            }
            return list;
        }

        public static void DetectNetworkAnomalies(MLContext mLContext)
        {
            var dataView = mlContext.Data.LoadFromTextFile<NetworksData>(
               DatasetPath,
               separatorChar: ',',
               hasHeader: true);

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            var transformedData = trainedModel.Transform(dataView);

            // Getting the data of the newly created column as an IEnumerable
            IEnumerable<NetworksDataPrediction> predictions =
                mlContext.Data.CreateEnumerable<NetworksDataPrediction>(transformedData, false);

            var colCDN = dataView.GetColumn<float>("MAXnetIN").ToArray();
            var colTime = dataView.GetColumn<DateTime>("key_as_string").ToArray();

            // Output the input data and predictions
            Console.WriteLine("======Displaying anomalies in the Power meter data=========");
            Console.WriteLine("Date              \tReadingDiff\tAlert\tScore\tP-Value");

            int i = 0;
            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine("{0}\t{1:0.0000}\t{2:0.00}\t{3:0.00}\t{4:0.00}",
                    colTime[i], colCDN[i],
                    p.Prediction[0], p.Prediction[1], p.Prediction[2]);
                Console.ResetColor();
                i++;
            }
        }

        public bool BuildTrainingModel()
        {
            var dataView = mlContext.Data.LoadFromTextFile<NetworksData>(
               DatasetPath,
               separatorChar: ',',
               hasHeader: true);

            // Configure the Estimator
            const int PValueSize = 30;
            const int SeasonalitySize = 30;
            const int TrainingSize = 90;
            const int ConfidenceInterval = 98;

            string outputColumnName = nameof(NetworksDataPrediction.Prediction);
            string inputColumnName = nameof(NetworksData.Host__Network__In__Bytes);

            var trainigPipeLine = mlContext.Transforms.DetectSpikeBySsa(
                outputColumnName,
                inputColumnName,
                confidence: ConfidenceInterval,
                pvalueHistoryLength: PValueSize,
                trainingWindowSize: TrainingSize,
                seasonalityWindowSize: SeasonalitySize);

            ITransformer trainedModel = trainigPipeLine.Fit(dataView);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, dataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);
            Console.WriteLine("");
            return true;
        }
    }
}