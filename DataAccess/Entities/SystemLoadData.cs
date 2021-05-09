using Nest;

namespace DataAccess.Entities.Load
{
    public class SystemLoadData
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }

        public float Value { get; set; }
        public System System { get; set; }
    }

    public class System
    {
        public Load Load { get; set; }
    }

    public class Load
    {
        public int number { get; set; }
    }
}