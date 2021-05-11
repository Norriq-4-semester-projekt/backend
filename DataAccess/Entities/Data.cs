namespace DataAccess.Entities
{
    public class Data
    {
        public string Timestamp { get; set; }

        public float Value { get; set; }

        public bool IsSpike { get; set; } = false;

        public string FieldType { get; set; }
    }
}