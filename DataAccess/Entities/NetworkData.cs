using Nest;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataAccess.Entities.Network
{
    public class NetworkData
    {
        [Text(Name = "@timestamp")]
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
        public Out Out { get; set; }

    }
    public class Out
    {
        public long Bytes { get; set; }
    }

    public class In
    {
        public long Bytes { get; set; }
    }
}


