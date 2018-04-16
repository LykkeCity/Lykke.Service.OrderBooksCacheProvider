using System;

namespace Core
{
    public class DbSettings
    {
        public string LogsConnString { get; set; }
    }

    public class CacheSettings
    {
        public string FinanceDataCacheInstance { get; set; }
        public string RedisConfiguration { get; set; }

        public string OrderBooksCacheKeyPattern { get; set; }
    }

    public static class CacheSettingsExt
    {
        public static string GetOrderBookKey(this CacheSettings settings, string assetPairId, bool isBuy)
        {
            return string.Format(settings.OrderBooksCacheKeyPattern, assetPairId, isBuy);
        }
    }

    public class MatchingOrdersSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public string HttpOrderBookPort { get; set; }
    }

    public class RabbitMqSettings
    {
        public string Host { get; set; }
        public string ExternalHost { get; set; }
        public string ExchangeOrderbook { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
    }

    public static class Ext
    {
        public static string GetConnectionString (this RabbitMqSettings settings)
        {
            return $"amqp://{settings.Username}:{settings.Password}@{settings.Host}:{settings.Port}";
        }

        public static Uri GetOrderBookInitUri(this MatchingOrdersSettings settings)
        {
            return new Uri($"http://{settings.IpEndpoint.InternalHost}:{settings.HttpOrderBookPort}/orderBooks");
        }
    }

    public class IpEndpointSettings
    {
        public string InternalHost { get; set; }
    }

    public class OrderBooksCacheProviderSettings
    {
        public DbSettings Db { get; set; }
        public MatchingOrdersSettings MatchingEngine { get; set; }
        public CacheSettings CacheSettings { get; set; }
    }

    public class AppSettings
    {
        public OrderBooksCacheProviderSettings OrderBooksCacheProvider { get; set; }
    }
}
