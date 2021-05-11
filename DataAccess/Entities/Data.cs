using Nest;

namespace DataAccess.Entities
{
    public class Data
    {
        [Date(Format = "DateTime")]
        public string Timestamp { get; set; }

        public float Value { get; set; }

        [Boolean(NullValue = false, Store = true)]
        public bool IsSpike { get; set; } = false;

        [Text(Name = "Type")]
        public string FieldType { get; set; }
    }
}