using Nest;
using System;

namespace DataAccess
{
    public class ElasticConnection
    {
        private static ElasticConnection _instance;
        private static readonly object Padlock = new object();
        public ElasticClient Client { get; }

        private ElasticConnection()
        {
            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200"));
            settings.BasicAuthentication("elastic", "changeme");
            //settings = new ConnectionSettings(new Uri("http://20.82.178.74:9200"));
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
            settings.DisableDirectStreaming();
            Client = new ElasticClient(settings);
        }

        public static ElasticConnection Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new ElasticConnection();
                    }
                    return _instance;
                }
            }
        }
    }
}