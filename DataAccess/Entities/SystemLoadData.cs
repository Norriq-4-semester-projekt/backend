using Nest;
using System;

namespace DataAccess.Entities.Load
{
    public class SystemLoadData
    {
        [Text(Name = "@timestamp")]
        public string Timestamp { get; set; }

        public System System { get; set; }
    }

    public class System
    {
        public Load Load { get; set; }
    }

    public class Load
    {
        public float i = 15;

        public float GetI()
        {
            return i;
        }

        public void SetI(float value)
        {
            i = value;
        }
    }
}