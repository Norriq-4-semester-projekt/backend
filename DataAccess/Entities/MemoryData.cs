using Nest;

namespace DataAccess.Entities.Memory
{
    public class MemoryData
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }

        public System System { get; set; }
    }

    public class System
    {
        public Memory Memory { get; set; }
    }

    public class Memory
    {
        public Actual Actual { get; set; }
    }

    public class Actual
    {
        public Used Used { get; set; }
    }

    public class Used
    {
        public long Bytes { get; set; }
    }
}