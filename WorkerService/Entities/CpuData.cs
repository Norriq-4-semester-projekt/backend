using Nest;

namespace WorkerService.Entities.Cpu
{
    public class CpuData
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }

        public Host Host { get; set; }
    }

    public class Host
    {
        public Cpu Cpu { get; set; }
    }

    public class Cpu
    {
        public float Pct { get; set; }
    }
}