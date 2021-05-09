using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.Entities
{
    internal class Data
    {
        [LoadColumn(0)]
        public string Timestamp { get; set; }

        [LoadColumn(1)]
        public float Value { get; set; }
    }
}
