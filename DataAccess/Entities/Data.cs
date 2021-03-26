using Newtonsoft.Json;

namespace DataAccess.Entities
{
    public class Data
    {
        private string _Timestamp;

        public string Timestamp
        {
            get => Timestamp;
            set => Timestamp = value;
        }

        public long Bytes
        {
            get => Bytes;
            set => Bytes = value;
        }

        //private float _AVGnetIN;

        //public float AVGnetIn
        //{
        //    get => _AVGnetIN;
        //    set => _AVGnetIN = value;
        //}

        //private float _AVGnetOut;

        //public float AVGnetOut
        //{
        //    get => AVGnetOut;
        //    set => AVGnetOut = value;
        //}

        //private float _MAXnetIN;

        //public float MAXnetIN
        //{
        //    get => MAXnetIN;
        //    set => MAXnetIN = value;
        //}

        //private float _MAXnetOUT;

        //public float MAXnetOUT
        //{
        //    get => MAXnetOUT;
        //    set => MAXnetOUT = value;
        //}

        public Data()
        {
        }

        public Data(string timestamp, /*float avgNetIn, float avgNetOut, float maxNetIn, float maxNetOut*/
            long Bytes)
        {
            this.Timestamp = timestamp;
            this.Bytes = Bytes;
            //this.AVGnetIn = avgNetIn;
            //this.AVGnetOut = avgNetOut;
            //this.MAXnetIN = maxNetIn;
            //this.MAXnetOUT = maxNetOut;
        }
    }

    public partial class Host
    {
        [JsonProperty("network")]
        public Network Network { get; set; }
    }

    public partial class Network
    {
        [JsonProperty("in")]
        public In In { get; set; }

        [JsonProperty("out")]
        public In Out { get; set; }
    }

    public partial class In
    {
        [JsonProperty("bytes")]
        public long Bytes { get; set; }

        [JsonProperty("packets")]
        public long Packets { get; set; }
    }
}