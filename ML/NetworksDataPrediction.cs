﻿using Microsoft.ML.Data;

namespace ML
{
    internal class NetworksDataPrediction
    {
        //vector to hold alert,score,p-value values
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}