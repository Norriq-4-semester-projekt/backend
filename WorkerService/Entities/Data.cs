using Microsoft.ML.Data;

namespace WorkerService.Entities
{
    internal class Data
    {
        [LoadColumn(0)]
        public string Timestamp { get; set; }

        [LoadColumn(1)]
        public float Value { get; set; }
    }
}