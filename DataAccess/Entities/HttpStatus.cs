using Nest;

namespace DataAccess.Entities
{
    public class HttpStatus
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }

        public string Status { get; set; }
    }

    //public class Host
    //{
    //    public Network Network { get; set; }
    //}

    //public class Network
    //{
    //    public In In { get; set; }
    //}

    //public class In
    //{
    //    public long Bytes { get; set; }
    //}
}