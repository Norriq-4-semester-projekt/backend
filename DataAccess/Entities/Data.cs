using Nest;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class Data
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
    }

    public class In
    {
        public long Bytes { get; set; }
    }
}