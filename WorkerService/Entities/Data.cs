using Microsoft.ML.Data;

namespace WorkerService.Entities
{
    public class Data
    {
        [LoadColumn(0)]
        public string Timestamp { get; set; }

        [LoadColumn(1)]
        public float Value { get; set; }

        public bool IsSpike { get; set; } = false;

        public string FieldType { get; set; }
    }
}