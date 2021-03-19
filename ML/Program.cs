using Microsoft.ML;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ML
{
    internal static class Program
    {
        private static string BaseDatasetsRelativePath = @"../../../../Data";

        //private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/product-sales.csv";
        private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/response.json";

        //private static string DatasetRelativePath = $"{BaseDatasetsRelativePath}/http.csv";

        private static string DatasetPath = GetAbsolutePath(DatasetRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/ProductSalesModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        private static MLContext mlContext;

        private static async System.Threading.Tasks.Task Main()
        {
            //DataContractJsonSerializer dj = new DataContractJsonSerializer(typeof(ProductsDataList));
            //MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(await File.OpenText(DatasetRelativePath).ReadToEndAsync()));
            //ProductsDataList training_data = (ProductsDataList)dj.ReadObject(ms);

            ProductsDataList training_data = UpdateMLModel();
            // Create MLContext to be shared across the model creation workflow objects
            mlContext = new MLContext();

            //assign the Number of records in dataset file to cosntant variable
            int size = training_data.Data.Count;

            //Load the data into IDataView.
            //This dataset is used while prediction/detecting spikes or changes.

            //IDataView dataView = mlContext.Data.LoadFromTextFile<ProductSalesData>(path: DatasetPath, hasHeader: true, separatorChar: ',');
            IDataView dataView = mlContext.Data.LoadFromEnumerable(training_data.Data);

            //To detech temporay changes in the pattern
            DetectSpike(size, dataView);

            //To detect persistent change in the pattern
            DetectChangepoint(size, dataView);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");

            Console.ReadLine();
        }

        private static void DetectSpike(int size, IDataView dataView)
        {
            Console.WriteLine("===============Detect temporary changes in pattern===============");

            //STEP 1: Create Esimtator
            var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(ProductSalesPrediction.Prediction), inputColumnName: nameof(ProductSalesData.doc_count), confidence: 99, pvalueHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            //In IID Spike detection, we don't need to do training, we just need to do transformation.
            //As you are not training the model, there is no need to load IDataView with real data, you just need schema of data.
            //So create empty data view and pass to Fit() method.
            ITransformer tansformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = tansformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);

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
            var estimator = mlContext.Transforms.DetectIidChangePoint(outputColumnName: nameof(ProductSalesPrediction.Prediction), inputColumnName: nameof(ProductSalesData.doc_count), confidence: 95, changeHistoryLength: size / 4);

            //STEP 2:The Transformed Model.
            //In IID Change point detection, we don't need need to do training, we just need to do transformation.
            //As you are not training the model, there is no need to load IDataView with real data, you just need schema of data.
            //So create empty data view and pass to Fit() method.
            ITransformer tansformedModel = estimator.Fit(CreateEmptyDataView());

            //STEP 3: Use/test model
            //Apply data transformation to create predictions.
            IDataView transformedData = tansformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<ProductSalesPrediction>(transformedData, reuseRowObject: false);

            Console.WriteLine($"{nameof(ProductSalesPrediction.Prediction)} column obtained post-transformation.");
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
            IEnumerable<ProductSalesData> enumerableData = new List<ProductSalesData>();
            var dv = mlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }

        public static ProductsDataList UpdateMLModel()
        {
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("packetbeat-*");
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
            settings.BasicAuthentication("elastic", "changeme");
            var client = new ElasticClient(settings);
            ProductsDataList pdl = new ProductsDataList();
            var rs = client.Search<dynamic>(s => s
            .Aggregations(aggs => aggs
            .DateHistogram("Data", dhg => dhg
            .Field("@timestamp")
            .CalendarInterval("1h")
            .TimeZone("Europe/Copenhagen")))
            .Size(0).Fields(fi => fi
            .Field("@timestamp"))
            .Query(q => q
            .Bool(b => b
            .Filter(f => f
            .MatchPhrase(mp => mp
            .Field("url.full").Query("http://thekrane.dk/")))
            .Filter(fil => fil.DateRange(dr => dr.Field("@timestamp").GreaterThanOrEquals("now-14d").LessThanOrEquals("now"))))));

            ProductsDataList list = new ProductsDataList();
            list.Data = new List<ProductSalesData>();
            if (rs.Aggregations.Count > 0)
            {
                foreach (DateHistogramBucket item in rs.Aggregations.DateHistogram("Data").Buckets)
                {
                    ProductSalesData pd = new ProductSalesData();
                    pd.doc_count = item.DocCount.Value;
                    pd.key_as_string = item.KeyAsString;
                    list.Data.Add(pd);
                }

                //DateHistogramBucket list = rs.Aggregations.DateHistogram("Data").Buckets;
                //list.Buckets.
                //rs.Aggregations.DateHistogram().Buckets
            }
            //return new ObjectResult(JsonSerializer.Serialize(list));
            return list;
        }
    }
}