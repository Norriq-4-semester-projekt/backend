using Nest;

namespace DataAccess.Entities.Cpu
{
    public class CpuData
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }

        public System System { get; set; }

        public float Value { get; set; }
    }

    public class System
    {
        public Cpu Cpu { get; set; }
    }

    public class Cpu
    {
        public User User { get; set; }
    }

    public class User
    {
        public long Pct { get; set; }
    }


}