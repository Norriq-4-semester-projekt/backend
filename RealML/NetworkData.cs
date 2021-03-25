using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealML
{
    class NetworkData
    {
        [LoadColumn(0)]
        public string Timestamp { get; set; }
        [LoadColumn(5), ColumnName("Label")]
        public float AVGnetOut { get; set; }
        [LoadColumn(8)]
        public float CpuCalc { get; set; }




    }
}
