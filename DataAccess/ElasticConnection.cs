using Nest;
using System;

namespace DataAccess
{
    public class ElasticConnection
    {
        private static ElasticConnection instance = null;
        private static readonly object padlock = new object();
        private static ConnectionSettings settings;
        private ElasticClient _client;
        public ElasticClient client { get => _client; }

        private ElasticConnection()
        {
            settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200"));
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
            settings.BasicAuthentication("elastic", "changeme");
            settings.DisableDirectStreaming();
            _client = new ElasticClient(settings);
        }

        public static ElasticConnection Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ElasticConnection();
                    }
                    return instance;
                }
            }
        }

        public string UpdateDefaultIndex(string index)
        {
            settings.DefaultIndex(index);
            return index;
        }
    }
}