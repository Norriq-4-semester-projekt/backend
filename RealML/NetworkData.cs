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
        [LoadColumn(1)]
        public float MAXnetOUT { get; set; }
        [LoadColumn(2)]
        public float MAXnetIN { get; set; }
        [LoadColumn(3), ColumnName("Label")]
        public float AVGnetOut { get; set; }
        [LoadColumn(4)]
        public float AVGnetIN { get; set; }


    }
}
