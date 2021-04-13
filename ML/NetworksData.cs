using Microsoft.ML.Data;

namespace ML
{
    public class NetworksData
    {
        [LoadColumn(0)]
        public string Timestamp;

        [LoadColumn(1)]
        public float Host__Network__In__Bytes;

        //[LoadColumn(2)]
        //public float MAXnetOUT;

        //[LoadColumn(3)]
        //public float AVGnetOUT;

        //[LoadColumn(4)]
        //public float AVGnetIN;
    }
}