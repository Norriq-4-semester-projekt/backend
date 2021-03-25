using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private float _AVGnetIN;
        public float AVGnetIn
        {
            get => _AVGnetIN;
            set => _AVGnetIN = value;
        }

        private float _AVGnetOut;
        public float AVGnetOut
        {
            get => AVGnetOut;
            set => AVGnetOut = value;
        }

        private float _MAXnetIN;
        public float MAXnetIN
        {
            get => MAXnetIN;
            set => MAXnetIN = value;
        }

        private float _MAXnetOUT;
        public float MAXnetOUT
        {
            get => MAXnetOUT;
            set => MAXnetOUT = value;
        }

        public Data() { }

        public Data(string timestamp, float avgNetIn, float avgNetOut, float maxNetIn, float maxNetOut)
        {
            this.Timestamp = timestamp;
            this.AVGnetIn = avgNetIn;
            this.AVGnetOut = avgNetOut;
            this.MAXnetIN = maxNetIn;
            this.MAXnetOUT = maxNetOut;
        }
    }
}
