using Microsoft.ML.Data;

namespace ML
{
    public class NetworksData
    {
        [LoadColumn(0)]
        public string Timestamp { get; set; }

        public Host Host { get; set; }
    }

    public class Host
    {
        public Network Network { get; set; }
    }

    public class Network
    {
        public In In { get; set; }
    }

    public class In
    {
        [LoadColumn(1)]
        public long Bytes { get; set; }
    }
}