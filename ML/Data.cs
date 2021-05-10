using Microsoft.ML.Data;

namespace ML
{
    internal class Data
    {
        [LoadColumn(0)]
        public string Timestamp { get; set; }

        [LoadColumn(1)]
        public float Bytes { get; set; }
    }
}