using Microsoft.ML.Data;

namespace WorkerService.Entities
{
    internal class Predictions
    {
        //vector to hold alert,score,p-value values
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}