using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Api.Models.v1_0
{
    public class Fields
    {
        public double ElapsedMilliseconds { get; set; }
        public int StatusCode { get; set; }
        public object ContentType { get; set; }
        public int ContentLength { get; set; }
        public string Protocol { get; set; }
        public string Method { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string PathBase { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string HostingRequestFinishedLog { get; set; }
        public EventId EventId { get; set; }
        public string SourceContext { get; set; }
        public string RequestId { get; set; }
        public string RequestPath { get; set; }
        public string ConnectionId { get; set; }
        public string MachineName { get; set; }
        public string Environment { get; set; }

        [JsonProperty("@timestamp")]
        public List<DateTime> Timestamp { get; set; }
    }
}