using Nest;

namespace DataAccess.Entities.Network
{
    public class NetworkData
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }
        public string test { get; set; }

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

    //public class Out
    //{
    //    [Text(Name = "15")]
    //    public float Number { get; set; }
    //}

    public class Out
    {

        public float Bytes { get; set; }
    }

    public class In
    {

        public float Bytes { get; set; }
    }
    
}