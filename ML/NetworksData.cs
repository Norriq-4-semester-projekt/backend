using Microsoft.ML.Data;

namespace ML
{
    public class NetworksData
    {
        [LoadColumn(0)]
        public string key_as_string;

        [LoadColumn(1)]
        public float MAXnetIN;

        //[LoadColumn(2)]
        //public float MAXnetOUT;

        //[LoadColumn(3)]
        //public float AVGnetOUT;

        //[LoadColumn(4)]
        //public float AVGnetIN;
    }
}