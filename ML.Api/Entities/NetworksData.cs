using Microsoft.ML.Data;

namespace ML.Api
{
    public class NetworksData
    {
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
        public long Bytes { get; set; }
    }
}