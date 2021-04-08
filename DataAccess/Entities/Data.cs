using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class Data
    {
        public string Timestamp { get; set; }

        public long BytesIn { get; set; }

        public List<Data> BytesList;

    }
}