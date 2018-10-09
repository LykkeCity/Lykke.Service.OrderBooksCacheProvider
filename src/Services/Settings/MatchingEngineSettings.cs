using System;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.OrderBooksCacheProvider.Services.Settings
{
    public class MatchingEngineSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public string HttpOrderBookPort { get; set; }

        public Uri GetOrderBookInitUri()
        {
            return new Uri($"http://{IpEndpoint.InternalHost}:{HttpOrderBookPort}/orderBooks");
        }
    }

    public class IpEndpointSettings
    {
        public string InternalHost { get; set; }
    }

    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string ExchangeOrderbook { get; set; }
    }
}