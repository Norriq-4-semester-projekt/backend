using Microsoft.ML;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealML
{
    class Program
    {
        private List<AllData> dataList = new List<AllData>();
        private AllData allData = new AllData();
        static void Main(string[] args)
        {
            var context = new MLContext();

            var data = context.Data.LoadFromTextFile<NetworkData>("./response.csv", hasHeader: true,
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

        public void Train()
        {

        }

        public void Evaluate()
        {

        }

        public void Test()
        {

        }

        public void getData()
        {
                try
                {
                    var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("metricbeat-*");
                    settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
                    settings.PrettyJson(); // Good for DEBUG
                    settings.BasicAuthentication("elastic", "changeme");
                    var client = new ElasticClient(settings);

                    var rs = client.Search<dynamic>(s => s
                        .Query(q => q
                            .Bool(b => b
                                .Should(sh => sh
                                    .MatchPhrase(mp => mp
                                        .Field("host.name").Query("vmi316085.contaboserver.net")
                                        .Field("event.dataset").Query("system.cpu")
                                    )
                                )
                                .Filter(f => f
                                    .DateRange(dr => dr
                                        .Field("@timestamp")
                                        .GreaterThanOrEquals("now-5m")
                                        )
                                    )
                                )
                            )
                        .Aggregations(aggs1 => aggs1
                            .DateHistogram("myCpuDateHistogram", date => date
                            .Field("@timestamp")
                            .CalendarInterval(DateInterval.Minute)
                            .Aggregations(aggs => aggs
                                .Average("AvgUserCpu", auc => auc
                                .Field("system.cpu.user.pct")
                                )
                            .Average("AvgSystemCpu", asc => asc
                            .Field("system.cpu.system.pct")
                            )
                            .Max("CpuCoresMax", mcpu => mcpu
                            .Field("system.cpu.cores")
                        ).BucketScript("CpuCalc", bs => bs
                            .BucketsPath(bp => bp
                            .Add("user", "AvgUserCpu")
                            .Add("system", "AvgSystemCpu")
                            .Add("cores", "CpuCoresMax"))
                            .Script("(params.user + params.system) / params.cores")))
                    )
                            )
                );
                    if (rs.Aggregations.Count > 0)
                    {
                        Dictionary<String, Object> etellerandet = new Dictionary<String, Object>();
                        var dateHistogram = rs.Aggregations.DateHistogram("myCpuDateHistogram");
                        List<Object> list = new List<Object>();

                        foreach (DateHistogramBucket item in dateHistogram.Buckets)
                        {
                            item.TryGetValue("CpuCalc", out IAggregate a);
                        ValueAggregate valueAggregate = a as ValueAggregate;
                        allData.CpuCalc = (float)valueAggregate.Value;
                        list.Add(a);
                        Dictionary<string, string> newlist = new Dictionary<string, string>();
                            Dictionary<string, string> cpuPair = new Dictionary<string, string>();

                            foreach (var test89 in item.Keys)
                            {
                                item.TryGetValue(test89, out IAggregate a);
                                ValueAggregate valueAggregate = a as ValueAggregate;

                                cpuPair.Add(test89, valueAggregate.Value.ToString());
                                Console.WriteLine(test89 + ": " + (valueAggregate.Value));
                                Console.WriteLine(test89 + ": " + valueAggregate.ValueAsString);
                            }
                            list.Add(cpuPair);
                            //StatsAggregate test = (StatsAggregate)item.Values.FirstOrDefault();
                            //newlist.Add("Timestamp", item.Date.ToString());
                            //newlist.Add("min", test.Min.ToString());
                            //newlist.Add("max", test.Max.ToString());
                            //newlist.Add("avg", test.Average.ToString());
                            //list.Add(newlist);
                        }

                        return Ok(JsonSerializer.Serialize(list));
                    }
                    return new StatusCodeResult(200);
                }
                catch (Exception)
                {
                    return new StatusCodeResult(500);
                }
        }
    }
}
