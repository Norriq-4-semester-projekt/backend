using Newtonsoft.Json;

namespace DataAccess.Entities
{
    public class Data
    {
        public string Timestamp
        {
            get => Timestamp;
            set => Timestamp = value;
        }

        public long Bytes { get; set; }
    }
}