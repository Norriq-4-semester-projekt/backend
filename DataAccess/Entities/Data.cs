using Nest;

namespace DataAccess.Entities
{
    public class Data
    {
        [Nest.Date(Name = "timestamp", Format = "date_time_no_millis")]
        public string Timestamp { get; set; }

        public float Value { get; set; }

        [Boolean(NullValue = false, Store = true)]
        public bool IsSpike { get; set; } = false;

        [Text(Name = "Type")]
        public string FieldType { get; set; }
    }
}