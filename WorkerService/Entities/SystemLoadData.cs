using Nest;

namespace WorkerService.Entities.Load
{
    public class SystemLoadData
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }

        public System System { get; set; }
    }

    public class System
    {
        public Load Load { get; set; }
    }

    public class Load
    {
        [Text(Name = "15")]
        public float Number { get; set; }
    }
}